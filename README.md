# Between fire and water

RPG 2D por turnos previsto para Unity e C#, com investigação de mapas e evolução por XP, baseado no manuscrito **Entre fogo e água** fornecido pela equipe.

## Entrega 1 - Concepção | v0.1

Etapa prevista para **03/09/2026**, preparada em **10/09/2026**. Esta versão contém planejamento; ainda não há protótipo, build ou projeto Unity.

- [GDD inicial editável](docs/GDD-v0.1.md)
- [GDD inicial para entrega em PDF](output/pdf/Between-fire-and-water-GDD-v0.1.pdf)
- [Referência narrativa](docs/REFERENCIAS.md)

## Equipe

| Integrante | Responsabilidades propostas, a validar |
| --- | --- |
| Larissa Campos Cardoso | Game Design e produção |
| Izak Pinheiro | Arte 2D, animação, level design e áudio |
| Matheus Henrique | Programação e QA |

## Proposta de escopo

Campanha linear de 20 a 30 minutos, três mapas, seis encontros e um final. Magnus como personagem controlável; aliados em diálogos e eventos. Público 13+; PC e mobile, com Windows e Android propostos como primeiros alvos. Os valores e o volume de conteúdo serão validados no protótipo.

## Versionamento

Usar este mesmo repositório até a entrega final, incluindo o futuro projeto Unity, assets e respectivos arquivos `.meta`, documentos, mapas e registros de testes. As pastas geradas pela Unity estão no `.gitignore`.

Marcos: v0.1 concepção; v0.2 direção visual; v0.3 protótipo; v0.4 vertical slice; v0.5 beta; v1.0 final. A tag local v0.1 identifica esta concepção. O envio acadêmico também exige disponibilizar o commit e a tag em um repositório remoto acessível ao professor.

## Atualizar a documentação

O texto do GDD está centralizado em `tools/gerar_gdd.py`, que gera Markdown e PDF com o mesmo conteúdo. Requer Python 3 e ReportLab. Execute `python tools/gerar_gdd.py` na raiz e revise o PDF após mudanças.

## Próximos passos

Validar responsabilidades e escopo; informar os créditos do livro; disponibilizar o repositório remoto; criar concepts, model sheets e planejamento visual da v0.2; iniciar o protótipo Unity da v0.3. Não há resultados de testes de gameplay nesta entrega.
