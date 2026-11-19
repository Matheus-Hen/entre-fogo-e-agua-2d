# Entre Fogo e Água 3D

Jogo 3D de ação e aventura em terceira pessoa, feito em Unity 6 e C# para Projeto de Jogos Digitais II (V2). Magnus controla fogo e água e invade a base da ASDA para reunir três provas. Guardas e barreiras só cedem ao elemento certo.

## Documentos

- [GDD](docs/GDD.md): conceito, controles, mecânicas, pontuação, fase, decisões datadas e mapa dos requisitos no código.
- [Como abrir e jogar](docs/COMO-ABRIR-PROTOTIPO.md): execução, percurso de demonstração e testes.
- [Referências e créditos](docs/REFERENCIAS.md).

## Abrir o jogo

Abra `Between fire and water/` no Unity Hub (Unity 6000.6.0f1), carregue `Assets/Scenes/Menu.unity` e pressione Play.

## Estrutura

- `Between fire and water/`: projeto Unity.
  - `Assets/Scenes/`: `Menu`, `Fase01_BaseASDA`. Os protótipos antigos `Cena01_Prologo` e `Acampamento` não entram no build.
  - `Assets/scripts/Jogo3D/`: scripts do jogo (assembly `EntreFogoEAgua`) e `Editor/ConstrutorJogo3D.cs`, o gerador de cenas e prefabs.
  - `Assets/Prefabs/`, `Assets/Animations/`, `Assets/Materials/Jogo3D/`, `Assets/Models/`, `Assets/Audio/`.
  - `Assets/Tests/PlayMode/`: 12 testes de Play Mode.
- `Art/Modelos3D/`: fontes `.blend` editáveis (Magnus e props). `Art/ModelSheets/`: model sheets da equipe.
- `tools/blender/` e `tools/audio/`: scripts que geram modelos, animações e áudio.
- `Art/`, `LevelDesign/`, `imagens/`: acervo visual da versão 2D.

## Verificação mais recente (19/11/2026)

Os scripts compilam sem erros. Os 12 testes de Play Mode passaram, cobrindo pontuação sem duplicar, afinidade elemental, barreiras, placa e ponte, queda no rio, vitória, derrota com reinício e o botão Jogar do menu. Ainda faltam uma partida manual completa, um playtest externo e uma build (opcional).
