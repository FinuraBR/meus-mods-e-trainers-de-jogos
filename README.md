# Paper Lily - Chapter 1

## Mod: Quicksave System (F5)
Este mod permite salvar o jogo a qualquer momento durante a gameplay pressionando a tecla **F5**. O sistema busca automaticamente por um slot vazio e, caso todos estejam ocupados, sobrescreve o save mais antigo baseado na data de modificação do arquivo.

---

## Arquivos nesta pasta
Para transparência e para facilitar futuras modificações, os seguintes arquivos estão disponíveis diretamente nesta pasta:
*   `Player.cs`: O código-fonte em C# contendo a lógica da tecla F5 injetada.
*   `Lacie Engine.dll`: A DLL original do motor do jogo já modificada (pode ser usada para instalação manual).
*   `README.md`: Este guia de instalação e documentação.

---

## Instalação (Usuário)
1. Vá até a pasta raiz do seu jogo.
2. **Importante:** Faça um backup do seu arquivo original `PaperLilyCh1.exe` renomeando-o para `PaperLilyCh1.exe.bak`.
3. Baixe o arquivo modificado `PaperLilyCh1.exe` na aba de [Releases](https://github.com/FinuraBR/meus-mods-de-jogos/releases/tag/Paper-Lily-Chapter-1).
4. Coloque o executável baixado na pasta raiz do jogo.
5. Inicie o jogo normalmente.

#### Como usar
*   Pressione **F5** a qualquer momento enquanto controla a personagem.
*   Um ícone de salvamento aparecerá no canto da tela.
*   O mod salvará nos slots padrão do jogo (`slot1`, `slot2`, etc.).

---

## Documentação Técnica (Para Desenvolvedores/Atualizações)

Caso o jogo receba atualizações oficiais, o `PaperLilyCh1.exe` modificado pode parar de funcionar. Abaixo está o roteiro técnico para refazer o mod:

#### 1. Ferramentas Utilizadas
*   **Godot RE Tools:** Para extração do conteúdo binário do executável.
*   **dnSpy:** Para engenharia reversa e injeção de código C# (IL) na DLL do motor.

#### 2. Processo de Modificação
O código foi injetado na DLL principal do jogo: `Lacie Engine.dll`.

1.  **Localização:** O arquivo original reside internamente no executável ou no pacote `.pck` em `.mono/assemblies/Release/`.
2.  **Classe Alvo:** `LacieEngine.Nodes.Player`.
3.  **Método Alvo:** `_Input(InputEvent @event)`.
4.  **Lógica:** Captura do Scancode `16777248` (F5), verificação de slots via `System.IO` e chamada da função nativa `LacieEngine.Core.GameState.Save`.

#### 3. Notas de Compilação (Correção de Bugs de Descompilação)
Ao recompilar a classe `Player` no dnSpy, é necessário corrigir erros gerados pela ferramenta:
*   **Interface IPlayer:** Adicionar `public bool Exists { get { return Godot.Object.IsInstanceValid(this); } }`.
*   **Ambiguidade de Objeto:** Alterar referências de `(Object)` para `(Godot.Object)`.

#### 4. Código Injetado (C#)
```csharp
if (@event is InputEventKey keyEvent && keyEvent.Pressed && !keyEvent.Echo && keyEvent.Scancode == 16777248U)
{
    string slotDestino = "slot1";
    int maxSlots = 30;
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
