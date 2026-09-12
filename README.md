# Between fire and water

RPG 2D por turnos previsto para Unity e C#, baseado em **Entre fogo e água**. Campanha proposta de 20 a 30 minutos, três mapas e seis encontros. Magnus é o personagem controlável. Alvos: Windows e Android; público 13+.

## Entrega 2 - Direção Visual e Level Design | v0.2

**10/09/2026 | Valor: 1,0 ponto.** Pré-produção; ainda não há build nem projeto Unity.

- [GDD v0.2 em PDF](output/pdf/Between-fire-and-water-GDD-v0.2.pdf)
- [GDD v0.2 editável](docs/GDD-v0.2.md)
- [Catálogo das dez imagens](imagens/README.md)
- [Core loop visual e diagrama de fluxo](imagens/07-diagramas/README.md), incorporados nas páginas 5 e 6 do GDD em revisão de 11/09/2026.

| Requisito | Material |
| --- | --- |
| Concept art dos principais elementos | [Fogo e água](imagens/01-concept-art/), [Magnus](imagens/02-model-sheet/) e [arena ASDA](imagens/04-cenarios/) |
| Model sheet do protagonista | [Quatro vistas de Magnus](imagens/02-model-sheet/), reproduzidas no GDD como prancha preliminar |
| Referências e identidade visual | [Painel interno, paleta e estilo](imagens/03-identidade-visual/README.md) e GDD |
| Planejamento de sprites e animações | [Frames, FPS e preparação](imagens/06-sprites-animacoes/README.md) e GDD |
| Conceitos de cenário | Arena ilustrada, plantas e linguagem visual das três fases no GDD |
| Mapa, sketch ou planejamento das fases | [Três plantas](imagens/05-level-design/) e rotas com bloqueios no GDD |
| Progressão dos desafios | GDD: seis encontros, aprendizagem, XP proposto e critérios de teste |
| GDD atualizado | Markdown e PDF v0.2 acima |
| Core loop visual e fluxo do jogo | [Ciclo principal](imagens/07-diagramas/core-loop.png) e [fluxograma](imagens/07-diagramas/fluxo-do-jogo.png), incluídos no GDD |
| Repositório com tag v0.2 | Tag do commit desta entrega; consultar no [GitHub](https://github.com/Matheus-Hen/entre-fogo-e-agua-2d) |

## Organização e status

As seis pastas originais em `imagens/` separam concepts, model sheet, identidade, cenários, level design e animações. A pasta `07-diagramas` acrescenta os dois visuais solicitados pelo professor. As pastas de identidade e animações contêm guias que referenciam as artes, sem duplicá-las. O [inventário](imagens/inventario.json) registra nomes originais e SHA-256. Os dez PNGs originais foram preservados sem alteração de pixels.

As artes são referências de pré-produção. Faltam alinhamento e recorte dos sprites, turnaround neutro final, animações de combate, concepts dos demais personagens e arte final dos cenários 1 e 2. XP, dimensões e FPS são propostas a testar. Confirmar autoria e licenças das imagens e do manuscrito.

## Equipe

| Integrante | Responsabilidades propostas, a validar |
| --- | --- |
| Larissa Campos Cardoso | Game Design e produção |
| Izak Pinheiro | Arte 2D, animação, level design e áudio |
| Matheus Henrique | Programação e QA |

## Histórico e reprodução

- v0.1: [GDD inicial](docs/GDD-v0.1.md) e [PDF original](output/pdf/Between-fire-and-water-GDD-v0.1.pdf), preservados.
- v0.2: direção visual e level design, 10/09/2026.
- Revisão da v0.2 em 11/09/2026: core loop visual e fluxo do jogo após feedback do professor.
- Próximos marcos: v0.3 protótipo; v0.4 vertical slice; v0.5 beta; v1.0 final.

Executar `python tools/gerar_entrega_v02.py` na raiz com Python 3, ReportLab e Pillow para gerar a v0.2. O script centraliza o conteúdo novo e reutiliza a concepção de `tools/gerar_gdd.py`; produz Markdown e PDF equivalentes. Revisar visualmente após mudanças. `python tools/gerar_gdd.py` regenera somente a v0.1.

Para alterar os diagramas, editar `tools/gerar_diagramas.py` e executar `python tools/gerar_diagramas.py` (requer ReportLab e Poppler no PATH); depois regenerar o GDD v0.2. PNG, SVG e HTML são gerados a partir da mesma definição.

Manter o futuro projeto Unity, assets e `.meta` neste repositório. Ver [referências](docs/REFERENCIAS.md). Próximo passo: implementar movimento, interação e uma batalha na v0.3; registrar testes em PC e Android.
