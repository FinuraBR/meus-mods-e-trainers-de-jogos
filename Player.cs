using System;
using System.Collections.Generic;
using System.IO;
using Godot;
using LacieEngine.API;
using LacieEngine.Core;

namespace LacieEngine.Nodes
{
	// Token: 0x02000210 RID: 528
	public class Player : WalkingCharacter, IPlayer, IPhysicsInterpolated, ITurnable, IWalker, IReflectable, IFollowable
	{
		// Token: 0x17000212 RID: 530
		// (get) Token: 0x06000D89 RID: 3465 RVA: 0x0000BE88 File Offset: 0x0000A088
		// (set) Token: 0x06000D8A RID: 3466 RVA: 0x0000BE9A File Offset: 0x0000A09A
		public Direction SpriteDirection
		{
			get
			{
				return this.nSprite.Direction;
			}
			set
			{
				this.nSprite.Direction = value;
			}
		}

		// Token: 0x17000213 RID: 531
		// (get) Token: 0x06000D8B RID: 3467 RVA: 0x0000BEAD File Offset: 0x0000A0AD
		// (set) Token: 0x06000D8C RID: 3468 RVA: 0x0000BEB5 File Offset: 0x0000A0B5
		public List<IFollowable.Segment> FollowableSegments { get; private set; }

		// Token: 0x06000D8D RID: 3469 RVA: 0x0000BEBE File Offset: 0x0000A0BE
		public override void _EnterTree()
		{
			base.CharacterId = Game.State.Party[0];
			base.CreateCharacterSprite();
			base.DefaultDirection = Direction.Down;
			base.PlayerCharacter = true;
		}

		// Token: 0x06000D8E RID: 3470 RVA: 0x0003B3F8 File Offset: 0x000395F8
		protected override void _CharacterReady()
		{
			this.nInteractions = (base.GetNode("InteractionBox") as Area2D);
			this.nThinInteractions = (base.GetNode("ThinInteractionBox") as Area2D);
			this.nFloorDetection = (base.GetNode("FloorDetection") as Area2D);
			this.nCenter = (base.GetNode("Center") as Position2D);
			this.nDirectionalInteractions = new Dictionary<Direction, CollisionShape2D>();
			this.nDirectionalInteractions.Add(Direction.Up, this.nInteractions.GetNode("Up") as CollisionShape2D);
			this.nDirectionalInteractions.Add(Direction.Down, this.nInteractions.GetNode("Down") as CollisionShape2D);
			this.nDirectionalInteractions.Add(Direction.Left, this.nInteractions.GetNode("Left") as CollisionShape2D);
			this.nDirectionalInteractions.Add(Direction.Right, this.nInteractions.GetNode("Right") as CollisionShape2D);
			this.nThinDirectionalInteractions = new Dictionary<Direction, CollisionShape2D>();
			this.nThinDirectionalInteractions.Add(Direction.Up, this.nThinInteractions.GetNode("Up") as CollisionShape2D);
			this.nThinDirectionalInteractions.Add(Direction.Down, this.nThinInteractions.GetNode("Down") as CollisionShape2D);
			this.nThinDirectionalInteractions.Add(Direction.Left, this.nThinInteractions.GetNode("Left") as CollisionShape2D);
			this.nThinDirectionalInteractions.Add(Direction.Right, this.nThinInteractions.GetNode("Right") as CollisionShape2D);
			this.nThinInteractions.Connect("area_entered", this, "CheckAndRunInstantEvents", null, 0U);
			this.SetPlayerSprite();
			this.Turn();
			this.FollowableSegments = new List<IFollowable.Segment>
			{
				new IFollowable.Segment(this.Direction, base.Position)
			};
			this._currentFollowableSegment = this.FollowableSegments[0];
			Game.Room.RegisterMirrorReflection(this);
			this._collisionLayer = base.CollisionLayer;
			this._collisionMask = base.CollisionMask;
			this.Ready = true;
		}

		// Token: 0x06000D8F RID: 3471 RVA: 0x0003B65C File Offset: 0x0003985C
		public override void _Input(InputEvent @event)
		{
			InputEventKey inputEventKey;
			if ((inputEventKey = (@event as InputEventKey)) != null && inputEventKey.Pressed && !inputEventKey.Echo && inputEventKey.Scancode == 16777248U)
			{
				string text = "slot1";
				int num = 30;
				bool flag = false;
				for (int i = 1; i <= num; i++)
				{
					string text2 = "slot" + i;
					if (!GameState.SaveExists(text2))
					{
						text = text2;
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					try
					{
						FileInfo[] files = new DirectoryInfo(ProjectSettings.GlobalizePath("user://save/")).GetFiles("slot*.sav");
						if (files.Length != 0)
						{
							FileInfo fileInfo = files[0];
							foreach (FileInfo fileInfo2 in files)
							{
								if (fileInfo2.LastWriteTime < fileInfo.LastWriteTime)
								{
									fileInfo = fileInfo2;
								}
							}
							text = System.IO.Path.GetFileNameWithoutExtension(fileInfo.Name);
						}
					}
					catch (Exception)
					{
						text = "slot1";
					}
				}
				GD.Print(new object[]
				{
					"MOD: Salvando inteligentemente no " + text
				});
				GameState.Save(text, false);
				return;
			}
			string a = Inputs.Handle(@event, Inputs.Processor.Player, Player.HandledInputs, false);
			if (!(a == "input_action"))
			{
				if (!(a == "input_menu"))
				{
					return;
				}
				if (!Game.State.MenuDisabled)
				{
					Game.Screen.OpenMenu();
					return;
				}
				Game.Audio.PlaySystemSound("res://assets/sfx/ui_bad.ogg");
				return;
			}
			else
			{
				EventTrigger interactingNode = this.GetInteractingNode();
				if (interactingNode != null)
				{
					if (!interactingNode.RelatedNode.IsNullOrEmpty())
					{
						ITurnable turnable = interactingNode.GetNode(interactingNode.RelatedNode) as ITurnable;
						if (turnable != null)
						{
							turnable.Turn(this.SpriteDirectionTemp.GetOpposite());
							Game.StoryPlayer.Connect("DialogueEnded", (Godot.Object)turnable, "TurnToDefault", null, 4U);
						}
					}
					Game.Events.ExecuteMapping(interactingNode.Name);
					return;
				}
				IActionObject interactingActionableNode = this.GetInteractingActionableNode();
				if (interactingActionableNode != null)
				{
					interactingActionableNode.Activate();
					return;
				}
				return;
			}
		}

		// Token: 0x06000D90 RID: 3472 RVA: 0x0003B860 File Offset: 0x00039A60
		public override void _PhysicsProcess(float delta)
		{
			if (!this.Ready)
			{
				return;
			}
			this._currentFollowableSegment.Destination = base.Position;
			if (this.AutoMovement)
			{
				this.NewDirection = this.AutoDirection;
			}
			else if (Game.InputProcessor == Inputs.Processor.Player)
			{
				this.NewDirection = Inputs.GetDirectionFromInput(this.Direction, Inputs.Processor.Player);
			}
			base.UpdateState();
			Vector2 inputMotion;
			Vector2 actualMotion;
			this.PlayerMotion(out inputMotion, out actualMotion);
			if (this.AutoMovement)
			{
				this.CheckIfAutoMovementOver();
			}
			if (this.NewDirection != Direction.None && Math.Abs(Mathf.Rad2Deg(this.NewDirection.ToVector().AngleTo(this._currentFollowableSegment.Direction.ToVector()))) >= 45f)
			{
				this.DelimitFollowableSegment();
			}
			if (Game.Characters.Get(base.CharacterId).IdleAnimation)
			{
				if (!Game.StoryPlayer.Running && this.nSprite.State != "idle" && Game.InputProcessor == Inputs.Processor.Player && inputMotion == Vector2.Zero)
				{
					this.IdleTime += delta;
				}
				else
				{
					this.IdleTime = 0f;
				}
				if (Game.InputProcessor == Inputs.Processor.Player && this.IdleTime > 10f)
				{
					this.IdleTime = 0f;
					this.IdleStart = true;
					this.State = CharacterState.Idle;
				}
			}
			this.PixelSnap(actualMotion.x == 0f, actualMotion.y == 0f);
			base.PlayAnimations();
			this.TurnInteractions();
		}

		// Token: 0x06000D91 RID: 3473 RVA: 0x0003B9E8 File Offset: 0x00039BE8
		protected virtual void PlayerMotion(out Vector2 inputMotion, out Vector2 actualMotion)
		{
			inputMotion = base.EightDirectionalMotion();
			actualMotion = base.MoveAndSlide(inputMotion, null, false, 4, 0.785398f, true);
		}

		// Token: 0x06000D92 RID: 3474 RVA: 0x0003BA24 File Offset: 0x00039C24
		public override Vector2 GetMovementWithFloorModifier(Vector2 movement)
		{
			foreach (object obj in this.nFloorDetection.GetOverlappingAreas())
			{
				IFloorMovementModifier floor = ((Node2D)obj) as IFloorMovementModifier;
				if (floor != null)
				{
					return floor.GetModifiedMovement(movement);
				}
			}
			return movement;
		}

		// Token: 0x06000D93 RID: 3475 RVA: 0x0000BEEE File Offset: 0x0000A0EE
		public void DisableRunning()
		{
			base.RunningDisabled = true;
		}

		// Token: 0x06000D94 RID: 3476 RVA: 0x0000BEF7 File Offset: 0x0000A0F7
		public void EnableRunning()
		{
			base.RunningDisabled = false;
		}

		// Token: 0x06000D95 RID: 3477 RVA: 0x0000BF00 File Offset: 0x0000A100
		public Direction GetDirection()
		{
			return this.Direction;
		}

		// Token: 0x06000D96 RID: 3478 RVA: 0x0000BF08 File Offset: 0x0000A108
		public void SetDirection(Direction dir)
		{
			this.NewDirection = dir;
			base.UpdateSpriteDirection();
			this.State = CharacterState.Standing;
		}

		// Token: 0x06000D97 RID: 3479 RVA: 0x0000BF1E File Offset: 0x0000A11E
		private void Turn()
		{
			base.SpriteState = "stand";
			this.Direction = this.SpriteDirectionTemp;
			this.Turn(this.Direction);
		}

		// Token: 0x06000D98 RID: 3480 RVA: 0x0000BF43 File Offset: 0x0000A143
		public Vector2 GetCenter()
		{
			return this.nCenter.GlobalPosition;
		}

		// Token: 0x06000D99 RID: 3481 RVA: 0x0003BA90 File Offset: 0x00039C90
		private void TurnInteractions()
		{
			foreach (CollisionShape2D collisionShape2D in this.nDirectionalInteractions.Values)
			{
				collisionShape2D.SetDeferred("disabled", true);
			}
			foreach (CollisionShape2D collisionShape2D2 in this.nThinDirectionalInteractions.Values)
			{
				collisionShape2D2.SetDeferred("disabled", true);
			}
			this.nDirectionalInteractions[this.SpriteDirectionTemp].SetDeferred("disabled", false);
			this.nThinDirectionalInteractions[this.SpriteDirectionTemp].SetDeferred("disabled", false);
		}

		// Token: 0x06000D9A RID: 3482 RVA: 0x0000BF50 File Offset: 0x0000A150
		private void SetPlayerSprite()
		{
			base.CharacterId = Game.State.Party[0];
			base.SpriteState = "stand";
		}

		// Token: 0x06000D9B RID: 3483 RVA: 0x0003BB84 File Offset: 0x00039D84
		public IActionObject GetInteractingActionableNode()
		{
			foreach (object obj in this.nThinInteractions.GetOverlappingBodies())
			{
				IActionObject actionableObject = ((Node2D)obj) as IActionObject;
				if (actionableObject != null && actionableObject.IsValidForDirection(this.SpriteDirectionTemp))
				{
					return actionableObject;
				}
			}
			return null;
		}

		// Token: 0x06000D9C RID: 3484 RVA: 0x0003BBF8 File Offset: 0x00039DF8
		public EventTrigger GetInteractingNode()
		{
			List<EventTrigger> nodes = new List<EventTrigger>();
			foreach (object obj in this.nFloorDetection.GetOverlappingAreas())
			{
				EventTrigger et = ((Node2D)obj).GetParent() as EventTrigger;
				if (et != null && et.Trigger == EventTrigger.ETrigger.Action && Game.Events.HasMapping(et.Name, this.SpriteDirectionTemp))
				{
					nodes.Add(et);
				}
			}
			foreach (object obj2 in this.nThinInteractions.GetOverlappingBodies())
			{
				EventTrigger et2 = ((Node2D)obj2).GetParent() as EventTrigger;
				if (et2 != null && et2.Trigger == EventTrigger.ETrigger.Action && Game.Events.HasMapping(et2.Name, this.SpriteDirectionTemp))
				{
					nodes.Add(et2);
				}
			}
			if (nodes.Count == 1)
			{
				return nodes[0];
			}
			if (nodes.Count > 1)
			{
				EventTrigger max = nodes[0];
				for (int i = 1; i < nodes.Count; i++)
				{
					if (nodes[i].Priority > max.Priority)
					{
						max = nodes[i];
					}
				}
				return max;
			}
			if (nodes.IsEmpty<EventTrigger>())
			{
				nodes.Clear();
				foreach (object obj3 in this.nInteractions.GetOverlappingBodies())
				{
					EventTrigger et3 = ((Node2D)obj3).GetParent() as EventTrigger;
					if (et3 != null && et3.Trigger == EventTrigger.ETrigger.Action && Game.Events.HasMapping(et3.Name, this.SpriteDirectionTemp))
					{
						nodes.Add(et3);
					}
				}
				if (nodes.Count == 1)
				{
					return nodes[0];
				}
				if (OS.IsDebugBuild() && nodes.Count > 1)
				{
					Log.Warn(new object[]
					{
						"Attention: Player is facing 2 interactions which are too close from one another. Consider redrawing them."
					});
				}
			}
			return null;
		}

		// Token: 0x06000D9D RID: 3485 RVA: 0x0003BE28 File Offset: 0x0003A028
		public EventTrigger GetItemInteractingNode(string itemId)
		{
			List<EventTrigger> nodes = new List<EventTrigger>();
			foreach (object obj in this.nFloorDetection.GetOverlappingAreas())
			{
				EventTrigger et = ((Node2D)obj).GetParent() as EventTrigger;
				if (et != null && (et.Trigger == EventTrigger.ETrigger.Action || et.Trigger == EventTrigger.ETrigger.Item) && Game.Events.HasItemObjectMapping(itemId, et.Name))
				{
					nodes.Add(et);
				}
			}
			foreach (object obj2 in this.nThinInteractions.GetOverlappingBodies())
			{
				EventTrigger et2 = ((Node2D)obj2).GetParent() as EventTrigger;
				if (et2 != null && (et2.Trigger == EventTrigger.ETrigger.Action || et2.Trigger == EventTrigger.ETrigger.Item) && Game.Events.HasItemObjectMapping(itemId, et2.Name))
				{
					nodes.Add(et2);
				}
			}
			if (nodes.Count == 1)
			{
				return nodes[0];
			}
			if (nodes.Count > 1)
			{
				EventTrigger max = nodes[0];
				for (int i = 1; i < nodes.Count; i++)
				{
					if (nodes[i].Priority > max.Priority)
					{
						max = nodes[i];
					}
				}
				return max;
			}
			if (nodes.IsEmpty<EventTrigger>())
			{
				nodes.Clear();
				foreach (object obj3 in this.nInteractions.GetOverlappingBodies())
				{
					EventTrigger et3 = ((Node2D)obj3).GetParent() as EventTrigger;
					if (et3 != null && (et3.Trigger == EventTrigger.ETrigger.Action || et3.Trigger == EventTrigger.ETrigger.Item) && Game.Events.HasItemObjectMapping(itemId, et3.Name))
					{
						nodes.Add(et3);
					}
				}
				if (nodes.Count == 1)
				{
					return nodes[0];
				}
				if (OS.IsDebugBuild() && nodes.Count > 1)
				{
					Log.Warn(new object[]
					{
						"Attention: Player is facing 2 interactions which are too close from one another. Consider redrawing them."
					});
				}
			}
			return null;
		}

		// Token: 0x06000D9E RID: 3486 RVA: 0x0003C068 File Offset: 0x0003A268
		public void CheckAndRunInstantEvents(Area2D area)
		{
			if (!this.AutoMovement)
			{
				EventTrigger et = area.GetParent() as EventTrigger;
				if (et != null && et.Trigger == EventTrigger.ETrigger.Touch && Game.Events.HasInstantMapping(et.Name))
				{
					Game.Events.ExecuteInstantMapping(et.Name);
				}
			}
		}

		// Token: 0x06000D9F RID: 3487 RVA: 0x0000BF73 File Offset: 0x0000A173
		public void SetLight(Light2D light)
		{
			this.nLight.DeleteIfValid();
			if (light != null)
			{
				base.AddChild(light, false);
			}
			this.nLight = light;
		}

		// Token: 0x06000DA0 RID: 3488 RVA: 0x0000BF92 File Offset: 0x0000A192
		public Light2D GetLight()
		{
			return this.nLight;
		}

		// Token: 0x06000DA1 RID: 3489 RVA: 0x0003C0B8 File Offset: 0x0003A2B8
		public SpriteState GetReflectedSprite()
		{
			return new SpriteState
			{
				Name = base.Name,
				Texture = this.nSprite.Texture,
				Frame = this.GetReflectedFrame(),
				HFrames = this.nSprite.Hframes,
				VFrames = this.nSprite.Vframes,
				Pos = base.VisualNode.GlobalPosition
			};
		}

		// Token: 0x06000DA2 RID: 3490 RVA: 0x0003C128 File Offset: 0x0003A328
		private int GetReflectedFrame()
		{
			if (this.nSprite.State == "stand" || this.nSprite.State == "walk" || this.nSprite.State == "run")
			{
				Direction.Enum @enum = this.SpriteDirectionTemp.ToEnum();
				switch (@enum)
				{
				case Direction.Enum.Left:
					return this.nSprite.Frame + 9;
				case Direction.Enum.Up:
					return this.nSprite.Frame - 27;
				case Direction.Enum.UpLeft:
					break;
				case Direction.Enum.Right:
					return this.nSprite.Frame - 9;
				default:
					if (@enum == Direction.Enum.Down)
					{
						return this.nSprite.Frame + 27;
					}
					break;
				}
				return 0;
			}
			if (this.nSprite.State == "idle")
			{
				int result;
				if (this.SpriteDirectionTemp.ToEnum() == Direction.Enum.Up)
				{
					result = this.nSprite.Frame - this.nSprite.Hframes;
				}
				else
				{
					result = this.nSprite.Frame + this.nSprite.Hframes;
				}
				return result;
			}
			return 0;
		}

		// Token: 0x06000DA3 RID: 3491 RVA: 0x0000BF9A File Offset: 0x0000A19A
		public void SetPlayerState(CharacterState state)
		{
			this.State = state;
		}

		// Token: 0x06000DA4 RID: 3492 RVA: 0x0000BFA3 File Offset: 0x0000A1A3
		public CharacterState GetPlayerState()
		{
			return this.State;
		}

		// Token: 0x06000DA5 RID: 3493 RVA: 0x0000BFAB File Offset: 0x0000A1AB
		private void DelimitFollowableSegment()
		{
			this._currentFollowableSegment = new IFollowable.Segment(this.NewDirection, base.Position);
			this.FollowableSegments.Add(this._currentFollowableSegment);
		}

		// Token: 0x06000DA6 RID: 3494 RVA: 0x0000BFD5 File Offset: 0x0000A1D5
		public bool IsRunning()
		{
			return this.State == CharacterState.Running;
		}

		// Token: 0x17000214 RID: 532
		// (get) Token: 0x06000DA7 RID: 3495 RVA: 0x0000BFE0 File Offset: 0x0000A1E0
		// (set) Token: 0x06000DA8 RID: 3496 RVA: 0x0000BFEB File Offset: 0x0000A1EB
		public bool CollisionEnabled
		{
			get
			{
				return base.CollisionLayer > 0U;
			}
			set
			{
				if (!value)
				{
					base.CollisionMask = 0U;
					base.CollisionLayer = 0U;
					return;
				}
				base.CollisionLayer = this._collisionLayer;
				base.CollisionMask = this._collisionMask;
			}
		}

		// Token: 0x17000215 RID: 533
		// (get) Token: 0x06000DAB RID: 3499 RVA: 0x0000C017 File Offset: 0x0000A217
		public bool Exists
		{
			get
			{
				return Godot.Object.IsInstanceValid(this);
			}
		}

		// Token: 0x04000C8B RID: 3211
		private Area2D nInteractions;

		// Token: 0x04000C8C RID: 3212
		private Area2D nThinInteractions;

		// Token: 0x04000C8D RID: 3213
		private Area2D nFloorDetection;

		// Token: 0x04000C8E RID: 3214
		private Position2D nCenter;

		// Token: 0x04000C8F RID: 3215
		private Light2D nLight;

		// Token: 0x04000C90 RID: 3216
		private Dictionary<Direction, CollisionShape2D> nDirectionalInteractions;

		// Token: 0x04000C91 RID: 3217
		private Dictionary<Direction, CollisionShape2D> nThinDirectionalInteractions;

		// Token: 0x04000C92 RID: 3218
		private static readonly string[] HandledInputs = new string[]
		{
			"input_action",
			"input_menu"
		};

		// Token: 0x04000C93 RID: 3219
		private static readonly string[] HandledMovementInputs = new string[]
		{
			"input_up",
			"input_down",
			"input_left",
			"input_right"
		};

		// Token: 0x04000C94 RID: 3220
		private static readonly string[] RecordedInputs = new string[]
		{
			"input_up",
			"input_down",
			"input_left",
			"input_right",
			"input_run"
		};

		// Token: 0x04000C96 RID: 3222
		private IFollowable.Segment _currentFollowableSegment;

		// Token: 0x04000C97 RID: 3223
		private uint _collisionLayer;

		// Token: 0x04000C98 RID: 3224
		private uint _collisionMask;
	}
}
