## Paper Lily - Chapter 1

### Mod: Quicksave System (F5)
Este mod permite salvar o jogo a qualquer momento durante a gameplay pressionando a tecla **F5**. O sistema busca automaticamente por um slot vazio e, caso todos estejam ocupados, sobrescreve o save mais antigo.

#### Instalação (Usuário)
1. Vá até a pasta raiz do seu jogo (onde o jogo foi instalado pela Steam ou Itch.io).
2. **Importante:** Faça um backup do seu arquivo original `PaperLilyCh1.exe`.
3. Baixe o arquivo modificado `PaperLilyCh1.exe` presente nesta pasta.
4. Substitua o arquivo original pelo modificado.
5. Inicie o jogo normalmente.

#### Como usar
*   Pressione **F5** a qualquer momento enquanto controla a personagem.
*   Um ícone de salvamento aparecerá no canto da tela.
*   O mod salvará nos slots padrão do jogo (`slot1`, `slot2`, etc.).

---

### Documentação Técnica (Para Desenvolvedores/Atualizações)

Se o jogo atualizar ou se você quiser modificar este mod, aqui está o processo detalhado que foi realizado:

#### 1. Ferramentas Utilizadas
*   **Godot RE Tools (v2.4.0):** Para descompilação e extração dos arquivos do executável.
*   **dnSpy:** Para engenharia reversa e injeção de código C# na DLL do motor.

#### 2. Processo de Modificação
O código foi injetado na DLL principal do jogo: `Lacie Engine.dll`.

1.  **Extração:** O conteúdo foi extraído do executável original, focando na pasta `.mono/assemblies/Release/`.
2.  **Injeção de Código:** A classe modificada foi a `LacieEngine.Nodes.Player` dentro do método `_Input(InputEvent @event)`.
3.  **Lógica Implementada:**
    *   Captura do Scancode `16777248` (F5).
    *   Uso de `System.IO` para verificar as datas de modificação dos arquivos `.sav` em `user://save/`.
    *   Chamada da função nativa `LacieEngine.Core.GameState.Save(nomeSlot, false)`.

#### 3. Notas de Compilação (Correção de Erros)
Ao recompilar a classe `Player` no dnSpy, foram necessários os seguintes ajustes manuais devido a erros de descompilação:
*   **Erro CS0535 (Interface IPlayer):** Foi necessário adicionar manualmente a propriedade:
    `public bool Exists { get { return Godot.Object.IsInstanceValid(this); } }`
*   **Erro de Ambiguidade (Object):** Onde o código utilizava `(Object)`, foi alterado para `(Godot.Object)` para evitar conflito com o `System.Object` do C#.

#### 4. Código-Fonte do Mod (Snippet)
```csharp
if (@event is InputEventKey keyEvent && keyEvent.Pressed && !keyEvent.Echo && keyEvent.Scancode == 16777248U)
{
    string slotDestino = "slot1";
    int maxSlots = 15;
    bool achouVazio = false;

    for (int i = 1; i <= maxSlots; i++)
    {
        string nomeSlot = "slot" + i;
        if (!LacieEngine.Core.GameState.SaveExists(nomeSlot))
        {
            slotDestino = nomeSlot;
            achouVazio = true;
            break;
        }
    }

    if (!achouVazio)
    {
        try {
            System.IO.DirectoryInfo dir = new System.IO.DirectoryInfo(ProjectSettings.GlobalizePath("user://save/"));
            System.IO.FileInfo[] arquivos = dir.GetFiles("slot*.sav");
            if (arquivos.Length > 0)
            {
                System.IO.FileInfo maisAntigo = arquivos[0];
                foreach (System.IO.FileInfo arq in arquivos)
                    if (arq.LastWriteTime < maisAntigo.LastWriteTime) maisAntigo = arq;
                slotDestino = System.IO.Path.GetFileNameWithoutExtension(maisAntigo.Name);
            }
        } catch { slotDestino = "slot1"; }
    }
    LacieEngine.Core.GameState.Save(slotDestino, false);
    return;
}
```