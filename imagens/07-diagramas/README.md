# Core loop e fluxo do jogo

Revisão de 11/09/2026 em resposta ao feedback do professor. Os diagramas sintetizam as regras de [Mecânicas e core loop](../../docs/GDD-v0.2.md#mecânicas-e-core-loop), mantendo a paleta do projeto e o escopo de três mapas e seis encontros.

| Visual | Uso no GDD | Imagem | Fonte vetorial | HTML para Canva |
| --- | --- | --- | --- | --- |
| Core loop | Página 5: explorar, investigar, combater, receber XP, evoluir e avançar | [PNG](core-loop.png) | [SVG](core-loop.svg) | [HTML](core-loop.html) |
| Fluxo do jogo | Página 6: exploração, decisões de combate, vitória, retentativa e conclusão | [PNG](fluxo-do-jogo.png) | [SVG](fluxo-do-jogo.svg) | [HTML](fluxo-do-jogo.html) |

O fluxo usa retângulos para ações, losangos para decisões e conectores A/B/C para unir trechos sem cruzar as setas. O avanço depende dos objetivos da área; XP é concedido uma vez por encontro e a subida de nível depende do limiar. O inimigo só age se sobreviver à ação de Magnus. A derrota restaura o estado anterior ao encontro; o evento de Brian integra o final narrativo.

Os visuais foram construídos localmente a partir do GDD. A importação pelo conector do Canva foi tentada, mas não foi concluída devido à repetição da autenticação. Não há design salvo no Canva nem link de edição confirmado. Os HTMLs contêm textos e formas separados e marcação de página para uma futura importação; a editabilidade após conversão ainda precisa ser verificada.

Para reproduzir, executar `python tools/gerar_diagramas.py` na raiz do repositório com ReportLab e `pdftoppm` (Poppler) no PATH. Depois executar `python tools/gerar_entrega_v02.py` para atualizar Markdown e PDF. A fonte de construção é [gerar_diagramas.py](../../tools/gerar_diagramas.py).
