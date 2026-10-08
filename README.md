# Between fire and water

Projeto de RPG 2D para um jogador, desenvolvido em Unity e C#. Acompanhe Magnus enquanto ele investiga a ASDA, controla fogo e água e enfrenta os inimigos em combates por turnos.

## Documentos

- [GDD consolidado](docs/GDD.md): conceito, narrativa, mecânicas, fases, direção visual, estado atual e pendências.
- [Como abrir o protótipo](docs/COMO-ABRIR-PROTOTIPO.md): instruções de execução na Unity.
- [Referências e créditos pendentes](docs/REFERENCIAS.md).

Este é o único README do projeto. O GDD consolidado é o único GDD ativo; versões antigas permanecem no histórico do Git.

## Abrir o jogo

Abra `Between fire and water/` no Unity Hub, carregue `Assets/Scenes/SampleScene.unity` e pressione Play. Os controles e o percurso estão descritos no guia de execução.

## Estrutura

- `Between fire and water/`: projeto Unity, cena, scripts, sprites e configurações.
- `docs/GDD.md`: documento principal do jogo.
- `Art/`: concepts, retratos, model sheets, fundos de batalha e estudos visuais.
- `LevelDesign/`: sketches e progressão visual das três fases.
- `imagens/`: material visual recebido para a entrega de planejamento.
- `Art/inventario.json` e `imagens/inventario.json`: inventários técnicos de origem e hashes.
- `output/pdf/`: guias visuais e materiais exportados que não são versões ativas do GDD.

## Verificação mais recente

Os scripts do jogo compilam sem erros. A suíte Play Mode anterior, de 08/10/2026, confirmou apenas o carregamento dos três fundos de exploração, os limites de colisão e a presença de Magnus. Testes ampliados para combate, derrota, transição e retomada do save estão em `Between fire and water/Assets/Tests/PlayMode/`; ainda precisam ser executados dentro da Unity. O relatório anterior está em `tmp/campaign-playmode-20261008-finalrun.xml`.

## Estado visual

O acervo inclui conceitos e estudos de personagens, cenários, animações e efeitos. Parte ainda precisa ser recortada, alinhada e integrada; as imagens conceituais não equivalem a fases navegáveis ou sprites finais. Consulte o GDD para os detalhes e pendências.
