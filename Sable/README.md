# Sable Cheat Engine Trainer

## Visão Geral

Este é um projeto de *reverse engineering* focado no desenvolvimento de um *trainer* para o jogo **Sable** utilizando o **Cheat Engine**. A ferramenta permite a manipulação de valores na memória do jogo em tempo real, concedendo ao jogador diversas facilidades e opções de depuração sem a necessidade de modificar os arquivos originais do jogo. O *trainer* foi projetado baseado na versão **4.3** do jogo.

## Compatibilidade

*   **Versão do Jogo:** Sable v4.3
*   **Plataforma:** PC
*   **Ferramenta Requerida:** [Cheat Engine](https://cheatengine.org/) (versão mais recente recomendada)

## Funcionalidades Incluídas

O *trainer* oferece um conjunto de *cheats* e parâmetros de depuração acessíveis diretamente pela interface do Cheat Engine. Marque as caixas para ativar os *cheats* ou ajuste os valores para modificar os parâmetros do jogo.

### Cheats Principais

*   **Inf Stamina:** Remove o consumo de stamina, permitindo sprint, escalada e outras ações por tempo ilimitado.
*   **Inf Compra ("Inf Dinheiro"):** Mesmo sem dinheiro suficiente, ainda é possivel comprar.

### Opções de Debug e Parâmetros do Jogo

Estes controles expõem variáveis internas do jogo, permitindo modificações diretas para fins de depuração ou para personalizar a experiência de jogo:

*   **Dinheiro:** Permite ajustar diretamente a quantidade de dinheiro do jogador.
*   **Qt Custom Marcador Mapa:** Controla a quantidade de marcadores personalizados que estão no mapa.
*   **Posição Peixe Minigame Pesca:** Exibe e potencialmente permite manipular a posição do peixe durante o minigame de pesca.
*   **Sprint Stamina Rate:** Ajusta a taxa de consumo de stamina ao usar o sprint.
*   **Minigame Pesca Finalizado:** Indica o estado de conclusão do minigame de pesca (0: Em Andamento/Não Pescando, 1: Finalizado).
*   **Estado Minigame Pesca:** Descreve a situação atual do peixe no minigame de pesca (0: Fora do Alvo, 1: No Alvo).
*   **Estado Pesca:** Detalha as diferentes fases do processo de pesca (e.g., Mirando, Carregando, Arremessando, Vitória, Falha).
*   **Força do Lançamento da Isca:** Controla a força com que a isca é lançada.
*   **Progresso Minigame Pesca:** Acelera ou congela o progresso no minigame de pesca.
*   **FPS Counter:** Ativa/desativa um contador de FPS interno do jogo (0: Off, 1: On).
*   **Fmod Global Parameter Debug:** Ativa/desativa o debug de parâmetros globais do sistema de áudio FMOD (0: Off, 1: On).
*   **Stamina Recharge Rate:** Modifica a velocidade com que a stamina do jogador é recarregada.
*   **Climbing Stamina:** Ajusta a quantidade de stamina consumida durante a escalada.
*   **Air Move Force:** Controla a força aplicada ao movimento aéreo do personagem.

## Arquivos do Projeto

*   `Sable.ct`: O arquivo principal da tabela do Cheat Engine, contendo todos os scripts de Auto Assembler e ponteiros de memória.
*   `README.md`: Este documento de guia de uso e documentação do projeto.

## Como Usar

Para utilizar o *trainer*, siga os passos abaixo:

1.  **Pré-requisito:** Certifique-se de ter o [Cheat Engine](https://cheatengine.org/) instalado em seu computador.
2.  Baixe o arquivo `Sable.ct` diretamente na aba de [Releases do repositório](https://github.com/FinuraBR/meus-mods-e-trainers-de-jogos/releases?q=Sable).
3.  Inicie o jogo **Sable**.
4.  Com o jogo rodando, dê um duplo clique no arquivo `Sable.ct` que você baixou. Isso abrirá a tabela no Cheat Engine.
5.  No Cheat Engine, clique no ícone de um **computador com uma lupa** (localizado no canto superior esquerdo e piscando).
6.  Na lista de processos, localize e selecione o executável do jogo **Sable.exe**.
7.  Quando o Cheat Engine perguntar se você deseja manter a lista atual de endereços/códigos, clique em **Sim**.
8.  Na parte inferior do programa, você verá as categorias "Cheats" e "Debug Options". Clique nas caixas de seleção (`[ ]`) ao lado dos *cheats* que deseja ativar ou altere os valores manualmente para os parâmetros desejados.

## Detalhes Técnicos

Este *trainer* é construído sobre técnicas avançadas de *reverse engineering* aplicadas a jogos que utilizam o motor Unity com backend IL2CPP.

### Metodologia de Reverse Engineering

1.  **Análise de IL2CPP:** Sable é um jogo Unity compilado com IL2CPP, o que significa que o código C# original é transpilado para C++ e então compilado para código nativo. Isso requer uma abordagem de *reverse engineering* de binários nativos (x64 assembly).
2.  **Escaneamento de AOB (Array of Bytes):** A técnica principal para localizar pontos de interesse no código do jogo é o escaneamento de padrões de bytes (`aobscanmodule`). Isso permite identificar sequências de instruções assembly específicas (`E8 E3 97 E5 FF`, `0F 9E C0 48 83 C4 28 C3 E8 87`, etc.) dentro do módulo `GameAssembly.dll`, que contém a lógica do jogo.

### Técnicas de Hooking e Injeção de Código

As modificações são implementadas através de scripts de **Auto Assembler** no Cheat Engine, que utilizam as seguintes técnicas:

1.  **Injeção de Trampolines:** Pequenos blocos de código (`jmp newmem_...`) são injetados nos endereços encontrados via AOB. Esses blocos desviam a execução para uma região de memória alocada dinamicamente (`alloc(newmem, ...)`) onde o código modificado reside. Após a execução do código customizado, a execução é retornada ao jogo (`jmp return_...`).
2.  **Modificação de Instruções:**
    *   **NOP (No Operation):** Para *cheats* como "Inf Stamina", instruções que decrementam a stamina são substituídas por `nop`s, efetivamente desativando a lógica de consumo.
    *   **XOR r8d, r8d:** Em alguns casos, uma instrução como `xor r8d,r8d` é usada para zerar um registrador que seria usado em um cálculo de stamina, garantindo que o valor permaneça máximo.
    *   **Ajuste do Stack Pointer (`add rsp,28`):** No "Inf Compra", a manipulação do `rsp` é usada para ignorar uma parte da função que provavelmente lidaria com a dedução de dinheiro ou uma checagem de saldo.
3.  **Alocação de Memória Global (`globalalloc`):** Para as "Opções de Debug", endereços de variáveis cruciais do jogo (como `dinheiro`, `posicao_peixe_minigame_pesca`, `sprintstaminarate`, etc.) são identificados em tempo de execução. O *trainer* injeta código para capturar o endereço dessas variáveis e armazená-lo em símbolos globais (`mov [simbolo], rax`), permitindo que o Cheat Engine os exiba e os torne editáveis na tabela.

### Exemplos de Implementação Específicos

*   **Inf Stamina:** Identifica o ponto onde a stamina é subtraída ou verificada em `SableCharacterController.set_CurrentClimbDistance` e `SableCharacterController.set_CurrentStamina` (referência baseada nos `CodeEntry` do XML), e injeta `NOP`s para anular a operação de decremento.
*   **Inf Compra:** Localiza a rotina de dedução de dinheiro e a instrução `setle al` (set if less or equal), que é tipicamente parte de uma validação de saldo. Ao modificar esta seção, o custo da compra é efetivamente ignorado.
*   **Exposição de Parâmetros de Debug:** Para variáveis como `Posição Peixe Minigame Pesca` ou `Sprint Stamina Rate`, o *trainer* intercepta as instruções que leem ou escrevem para essas variáveis (e.g., `movss xmm1,[rcx+0000021C]`) e, antes da operação original, salva o endereço da variável em uma região de memória global alocada. Isso permite que o usuário veja e modifique o valor diretamente.
