# Between fire and water | GDD v0.2

As especificações visuais abaixo detalham a concepção preservada nesta revisão.

## Concepção do jogo

### Identificação

Nome provisório: Between fire and water. Gênero: RPG 2D por turnos, para um jogador. Tecnologia prevista: Unity e C#. Público-alvo: pessoas a partir de 13 anos interessadas em fantasia, investigação e evolução de personagem. Plataformas-alvo: PC e mobile; Windows e Android são a proposta inicial de sistemas.

### Entrega e versão

Entrega 2 - Direção Visual e Level Design. Documento atualizado em 10/09/2026. Versão v0.2: concepção, direção visual e planejamento de fases. Imagens serão produzidas pela equipe em outra ferramenta com os prompts do guia visual; não há protótipo implementado.

### High Concept

Um jovem capaz de controlar fogo e água descobre que a agência que prometeu protegê-lo quer eliminá-lo. Em um RPG 2D por turnos baseado em Entre fogo e água, o jogador acompanha Magnus Lubros, investiga pistas sobre a ASDA, domina habilidades elementais e fortalece-se em batalhas até enfrentar a organização e descobrir quem realmente cumpre a profecia.

### Objetivo do jogador

Concluir o modo história: investigar a ameaça, superar os confrontos obrigatórios e chegar ao desfecho da queda da ASDA. A evolução por experiência permite enfrentar desafios progressivamente mais fortes.

### Pilares da experiência

Combate acessível: decisões claras em turnos. Descoberta: examinar o mapa revela pistas e desbloqueia objetivos. Progressão: vencer confrontos concede XP e aumenta o poder de Magnus. Narrativa: a campanha mantém o conflito e a revelação central do livro.

### Status das decisões

Nome, gênero, integrantes, público, plataformas, objetivo, combate, exploração e progressão foram informados pelo grupo. Divisão de papéis, quantidade de conteúdo, controles e regras numéricas deste GDD são propostas iniciais a validar e ajustar durante o desenvolvimento.

## Equipe e produção

### Larissa Campos Cardoso

Proposta de responsabilidade principal: Game Designer + Producer. Organizar conceito, roteiro adaptado, GDD, escopo, cronograma e acompanhamento das entregas. Apoiar testes de compreensão da história e dos objetivos.

### Izak Pinheiro

Proposta de responsabilidade principal: Artista 2D + Animador. Produzir concepts, model sheets, sprites, cenários, interface e animações. Acumular Level Design + Sound Design: planejar os três mapas e selecionar, produzir ou editar os efeitos e a trilha.

### Matheus Henrique

Proposta de responsabilidade principal: Programador + QA. Implementar exploração, combate, XP, interface, salvamento e integração na Unity. Gerar builds, registrar bugs e verificar controles, progressão e estabilidade em PC e mobile.

### Trabalho compartilhado

A equipe tem três integrantes; a divisão acima acumula áreas para cobrir a produção. Todos participam de playtests e revisões. A atribuição nominal ainda depende da validação do grupo e não comprova contribuições já realizadas. Cada integrante deverá registrar suas contribuições reais no histórico do projeto.

### Fluxo de trabalho proposto

Manter um único repositório com documentos, código e materiais produzidos. Usar tarefas pequenas com responsável e critério de conclusão. Atualizar o GDD quando houver mudança de regra ou escopo. Realizar uma revisão da versão antes de cada entrega e marcar a revisão correspondente com uma tag.

### Cronograma da disciplina

03/09 - v0.1: concepção e GDD inicial (entrega pendente). 10/09 - v0.2: direção visual e planejamento de fases. 17/09 - v0.3: protótipo jogável. 24/09 - v0.4: vertical slice com sistemas integrados. 01/10 - v0.5: conteúdo completo, playtesting e correções. 08/10 - v1.0: build final, GDD final e apresentação.

### Prioridades imediatas

Validar os papéis e o escopo; usar o guia visual para gerar concepts, model sheet e cenários; revisar e adicionar as imagens ao repositório. Iniciar uma cena de exploração conectada a uma batalha na Unity.

## História e adaptação

### Referência principal

Entre fogo e água, manuscrito fornecido pelo grupo em entre-fogo-e-agua.pdf, 21 páginas. Autoria e edição não foram informadas no pedido. As referências abaixo usam a numeração das páginas do arquivo PDF. O livro é fonte narrativa; as regras de jogo são propostas de adaptação.

### Mundo e conflito - páginas 1 a 6

O prólogo apresenta uma profecia sobre alguém ligado ao fogo e à água e uma organização criada para impedir sua realização. Na trama contemporânea, Magnus Lubros, de 14 anos, possui ligação com as duas nações. Ele e Lizzie entram em contato com a ASDA, que se apresenta como uma agência de proteção.

### Treinamento e traição - páginas 7 a 16

Magnus treina com Ki e aprende a controlar seus poderes. A confiança nos responsáveis pelo acampamento se desfaz quando a intenção de eliminá-lo é revelada. Ele foge, é capturado e escapa novamente com Lizzie, que também demonstra poderes.

### Aliados e investigação - páginas 16 a 19

Magnus e Lizzie encontram Michael, Olive e Brian. O grupo investiga a ASDA e identifica a ligação da empresa DASA com a organização. Esses acontecimentos fundamentam os objetivos de investigação e a progressão até a base inimiga.

### Desfecho - páginas 20 e 21 | spoilers

No confronto final, Brian se sacrifica e cria a oportunidade para os amigos reagirem e destruírem a ASDA. A revelação é que Brian, e não Magnus, cumpre a profecia em seu aniversário de 18 anos. O epílogo mostra os sobreviventes procurados após a destruição da empresa.

### Recorte para o jogo

Proposta: condensar a campanha em treinamento, investigação e confronto final. O prólogo e as viagens serão apresentados em diálogos curtos. Magnus será o único personagem controlável; Lizzie, Michael, Olive e Brian participarão por diálogos e eventos. Mark será adaptado como chefe final. O combate vencido conduz ao desfecho narrativo inspirado no livro, preservando a revelação sobre Brian.

### Tom e consistência

Adotar aventura e tensão com violência estilizada, sem gore e sem conteúdo sexual. Romance apenas sugerido. A faixa 13+ é uma intenção de público, não uma classificação oficial. Padronizar no roteiro os nomes usados de forma inconsistente no manuscrito e a idade de Brian conforme a revelação final. Usar áudio original ou com licença adequada; músicas citadas no livro não integram o escopo sonoro automaticamente.

## Mecânicas e core loop

### Mecânica principal: batalhas em turnos

Proposta inicial: Magnus enfrenta um inimigo por encontro. O jogador escolhe uma ação por turno: ataque básico, habilidade de fogo, habilidade de água ou defesa. Após a ação e a verificação de vida, o inimigo age se estiver vivo. A sequência se repete até a vitória ou derrota. Menus de ação pausam a decisão, sem limite de tempo.

### Fogo, água e defesa

O ataque básico não consome energia. A habilidade de fogo causa dano maior; a de água causa dano moderado e reduz o próximo dano recebido; defender reduz o próximo dano e recupera energia. Cada efeito defensivo expira após o próximo ataque recebido e não acumula. Valores, custos e multiplicadores serão definidos no protótipo. A interface deve mostrar custos, vida, energia e efeitos ativos.

### Mecânica secundária: investigação de mapa

Explorar mapas 2D com visão superior, conversar com NPCs e examinar objetos destacados. Pistas obrigatórias atualizam o objetivo e liberam a próxima área. Pistas opcionais acrescentam contexto. A aproximação de um inimigo visível inicia o combate; encontros obrigatórios vencidos não reaparecem.

### Experiência e evolução

Derrotar inimigos concede XP uma única vez por encontro. Atingir os limiares de XP aumenta o nível e melhora vida e poder de ataque. Proposta de limite: três níveis na campanha curta. O total de XP dos encontros obrigatórios deve permitir concluir o jogo sem repetir batalhas. Não haverá perda de nível após derrota.

### Core loop

Explorar o mapa > investigar pistas e identificar o objetivo > enfrentar e derrotar inimigos > receber XP > subir de nível e ficar mais forte > desbloquear a próxima etapa da história > explorar a nova área. O ciclo termina ao concluir o confronto final e o epílogo.

### Vitória e derrota

Na batalha, vence quem reduzir primeiro a vida do oponente a zero. A vida é limitada a zero no mínimo e a resolução interrompe as ações seguintes; não há ações simultâneas nesta proposta. Se Magnus chegar a zero, mostrar derrota e permitir reiniciar o encontro no estado anterior, sem duplicar XP. A vitória da campanha exige terminar os objetivos obrigatórios, vencer o chefe e assistir ao desfecho.

### Sistemas de apoio

Prever pausa, volume, retorno ao menu e um salvamento automático local ao concluir cada área. Ao carregar, restaurar nível, XP e objetivos concluídos. A cena narrativa de sacrifício de Brian não será tratada como derrota do jogador.

## Escopo inicial e experiência

### Campanha proposta

Duração estimada: 20 a 30 minutos, a confirmar em playtest. Três mapas pequenos, seis encontros no total e um único final. Uma campanha linear completa com começo, desenvolvimento e conclusão, condensando o livro para o prazo disponível.

### Mapa 1 - Acampamento e templo

Apresentar Magnus, Lizzie, Ki e o treinamento. Ensinar movimento, interação, ataque e defesa com um encontro de treino. A descoberta da traição abre uma rota de fuga com um guarda. Saída: alcançar o ponto de fuga após cumprir os objetivos. Total: dois encontros.

### Mapa 2 - Posto DASA

Apresentar os aliados por diálogos e investigar o vínculo da empresa com a ASDA. Encontrar duas pistas obrigatórias: identificação da organização e registro da base. Enfrentar dois guardas e liberar o caminho para o confronto final. Total: dois encontros.

### Mapa 3 - Base ASDA

Percorrer uma área curta até Mark. Vencer um guarda de elite e o chefe final; apresentar o sacrifício de Brian, a queda da ASDA e o epílogo em sequência narrativa. Total: dois encontros. O chefe é uma adaptação de gameplay, não uma descrição literal do combate do livro.

### Conteúdo e recursos previstos

Um personagem controlável; quatro aliados narrativos; Ki como instrutor e oponente de treino; Mark como antagonista. Reutilizar a base visual de guarda em variantes comum e elite. Prever sprites de exploração de Magnus, NPCs estáticos, retratos essenciais, poses de combate e efeitos simples de fogo e água. Animações mínimas: parado, andar, atacar, receber dano e derrota.

### Interface e controles

PC: WASD ou setas para mover, E para interagir, mouse para menus e Esc para pausar. Mobile: direcional virtual, botão de interação e seleção de ações por toque, em orientação horizontal. Prever menu inicial, objetivo atual, HUD de vida/energia/nível, menu de combate, vitória, derrota e créditos. Usar texto e ícones junto às cores de fogo e água.

### Limites de produção

Ficam fora da v1.0 proposta: mundo aberto, multiplayer, combate com equipe controlável, criação de itens, árvore extensa de habilidades, finais alternativos, dublagem e adaptação de todas as cenas do livro. PC e mobile continuam como alvos; validar Android cedo e priorizar a estabilização do PC. Se o prazo apertar, reduzir diálogos e variedade visual antes de remover história, investigação ou turnos.

## Validação e entrega v0.2

### Critérios para o protótipo v0.3

Mover Magnus com colisões; interagir com uma pista; iniciar e concluir uma batalha; impedir ações fora do turno; encerrar imediatamente quando a vida chegar a zero; conceder XP uma única vez; voltar ao mapa após vitória e reiniciar após derrota. Verificar também o toque em um aparelho Android disponível.

### Critérios para a versão final

Concluir os três mapas sem bloqueios; identificar o objetivo atual; alcançar o chefe com o XP dos encontros obrigatórios; terminar a história; salvar e retomar a progressão sem duplicar recompensas. Conferir legibilidade, controles, áudio e estabilidade em Windows e Android. Registrar aparelho, sistema, versão do jogo e resultado de cada teste.

### Playtesting previsto

Na beta, realizar sessões com pessoas externas ao desenvolvimento, registrar tempo de conclusão, pontos de dúvida e derrotas. Classificar bugs por impacto, corrigir primeiro travamentos e bloqueios de progressão e repetir os percursos afetados. Ainda não há playtests ou resultados nesta versão de concepção.

### Riscos e resposta

Escopo narrativo: condensar eventos nos três mapas. Sobrecarga de arte e level design: reutilizar tiles e variantes de guardas e redistribuir apoio entre integrantes. Mobile: testar resolução e toque desde o protótipo. Balanceamento: evitar dependência de repetição de encontros. Divergências do manuscrito: manter as decisões de adaptação registradas no GDD.

### Conteúdo desta entrega

GDD v0.2 em Markdown e guia visual em PDF e Markdown, contendo prompts, planejamento de sprites e animações e topologia das três fases. Artes e model sheets ainda não foram produzidos. Não há build nem projeto Unity implementado.

### Versionamento e envio

A tag v0.1 preserva a concepção e v0.2 registra o planejamento visual. O repositório permanece local, sem publicação remota. Adicionar as artes produzidas em novos commits e registrar novo marco após revisão.

### Pendências da equipe

Validar papéis; gerar e revisar as artes; confirmar créditos do livro; criar e publicar o repositório remoto; testar o escopo em Unity.

### Histórico

v0.1 - 10/09/2026: concepção para a etapa de 03/09. v0.2 - 10/09/2026: direção visual, prompts, planejamento de sprites, animações, fases e desafios.


# Direção visual e level design

## 01 | Direção visual

### Finalidade deste material

Entrega 2 - Direção Visual e Level Design | v0.2 | 10/09/2026. Este guia contém descrições e prompts para usar em outra ferramenta. Concepts, model sheets e sprites ainda precisam ser gerados, selecionados e revisados. As características visuais novas são propostas de adaptação; não são descrições literais do livro.

### Identidade proposta

Fantasia urbana juvenil em 2D: personagens expressivos, roupas contemporâneas e poderes elementais. Exploração com câmera superior levemente inclinada, sem perspectiva isométrica; batalhas com vista lateral. Usar ilustração com contorno e cores chapadas, evitando misturar pixel art com pintura realista. Silhuetas simples devem continuar reconhecíveis em telas pequenas.

### Paleta funcional

Azul-petróleo #152C3D: contornos, sombras e estrutura da interface. Ciano #32BBD3: água e proteção. Laranja #F47B35: fogo e ataque. Marfim #F3E9D2: fundos e áreas claras. Verde #637C57: vegetação. Cinza #748391: instalações da ASDA. Não usar apenas cor para comunicar: fogo tem forma de chama; água tem gota e arco fluido; interação tem ícone e contorno.

### Referências de conteúdo e forma

Fonte narrativa: manuscrito Entre fogo e água, 21 páginas, fornecido pelo grupo. Páginas 3-5 fundamentam Magnus; 7-11, treinamento e poderes; 16-19, aliados e investigação; 20-21, confronto final. Como referências formais, usar roupas juvenis contemporâneas sem marcas, templos de madeira e pedra, vegetação de acampamento e corredores de instalações de segurança. Essas categorias orientam o moodboard; ainda não foi montada uma prancha de imagens de referência.

### Prompt-base para todos os concepts

Arte conceitual para Between fire and water, RPG 2D por turnos de fantasia urbana juvenil. Ilustração digital estilizada, contornos limpos, sombreamento em duas ou três tonalidades, formas simples que possam ser convertidas em sprites. Paleta azul-petróleo, ciano, laranja e marfim. Design original, silhuetas legíveis, sem fotorrealismo, sem logos, sem marcas-d’água e sem texto na imagem. 

### Como usar

Para cada item, copie o prompt-base acima e acrescente o prompt específico das próximas páginas. Depois de aprovar Magnus, envie a mesma imagem como referência em todas as gerações dele. Preserve roupa, rosto, proporções e paleta. Gere cada prancha separadamente. As resoluções sugeridas são de saída; ajuste às opções da ferramenta sem cortar personagens.

## 02 | Magnus: concept principal

### Descrição e vínculo com o livro

Magnus Lubros é um adolescente de 14 anos, com cabelo escuro e olhos castanhos escuros, que controla fogo e água. O manuscrito o descreve com 1,73 m; no jogo, a escala será estilizada. A roupa abaixo é uma proposta para criar identidade própria e facilitar a animação.

### Elementos fixos do design

Corpo esguio juvenil, cerca de cinco cabeças e meia de altura na arte conceitual. Cabelo castanho muito escuro, curto e levemente bagunçado; olhos castanhos escuros. Jaqueta curta azul-petróleo aberta, camiseta marfim lisa, calça cinza-escura e tênis claros sem marcas. Duas faixas verticais estreitas, uma laranja e uma ciano, próximas ao zíper da jaqueta; ambos os braços têm mangas iguais. Não adicionar armadura, capa, armas ou detalhes pequenos.

### Prompt A - personagem inteiro

Magnus Lubros, adolescente de 14 anos, aparência juvenil, corpo esguio, cabelo castanho muito escuro curto e bagunçado, olhos castanhos escuros. Jaqueta curta azul-petróleo aberta com duas faixas finas laranja e ciano junto ao zíper, camiseta marfim sem estampa, calça cinza-escura, tênis claros. Corpo inteiro em vista frontal de três quartos, mãos visíveis, expressão determinada e curiosa, pose neutra confiante. Uma pequena chama flutua ao lado de uma mão e um arco de água ao lado da outra, sem cobrir o corpo. Fundo marfim liso, personagem centralizado com margem ampla. Formato vertical 2:3, sugestão 1024 x 1536.

### Prompt B - expressões

Usar a imagem aprovada de Magnus como referência obrigatória. Prancha com seis retratos do mesmo adolescente: neutro, confiante, desconfiado, surpreso, concentrado e triste. Preservar exatamente cabelo, olhos, roupa, idade e formato do rosto. Retratos separados em grade 3 por 2, mesma escala e mesmo enquadramento, fundo marfim, sem palavras. Formato 3:2, sugestão 1536 x 1024.

### Verificar antes de aprovar

Rosto e corpo devem parecer juvenis. Conferir mãos, quantidade de dedos e continuidade da roupa. As cores não podem trocar entre gerações. A chama e a água são efeitos separados, não partes permanentes da anatomia. Selecionar apenas um design-base antes de pedir o model sheet.

## 03 | Model sheet de Magnus

### Objetivo da prancha

Documentar o personagem para que todas as artes representem a mesma pessoa. Esta prancha é referência de construção; ainda não é uma folha de sprites pronta para Unity. Produzir as vistas sem efeitos elementais para não esconder roupa e proporções.

### Prompt C - vistas de construção

Usar o concept aprovado de Magnus como referência visual. Criar model sheet ortográfico do mesmo personagem em quatro vistas: frente, perfil esquerdo, costas e perfil direito. Corpo inteiro em todas as vistas, cabeça e sola dos pés alinhadas, escala e altura idênticas, postura neutra com braços ligeiramente afastados do tronco. Preservar a jaqueta azul-petróleo, camiseta marfim, calça cinza-escura, tênis claros, cabelo e olhos da referência. Sem perspectiva dramática, sem poderes, sem armas, sem sombras projetadas fortes, sem rótulos. Fundo claro uniforme, espaço igual entre figuras. Formato horizontal 2:1, sugestão 2048 x 1024.

### Prompt D - detalhes complementares

Usar o mesmo Magnus aprovado. Prancha de detalhes com três enquadramentos separados: cabeça de frente e de perfil; jaqueta de frente e de costas; mãos relaxadas e punhos fechados. Manter design e cores exatamente iguais à referência. Contorno limpo e acabamento simples, fundo marfim, sem texto, sem inventar acessórios. Formato 3:2, sugestão 1536 x 1024.

### Proporções e continuidade

A altura relativa das pernas, posição da cintura, tamanho da cabeça e comprimento das mangas devem coincidir nas vistas. Como a roupa usa poucos detalhes e mangas simétricas, ela facilita a produção de animação. Validar os perfis separadamente; espelhar uma imagem só é aceitável quando não altera detalhes de continuidade.

### Entrega visual esperada

Uma prancha de quatro vistas, uma prancha de expressões e uma prancha de detalhes. Após gerar, adicionar títulos e nomes manualmente em uma ferramenta de diagramação. Guardar o concept aprovado junto das pranchas para referência da equipe.

## 04 | Aliados, inimigos e objetos

### Uso e prioridade

Magnus tem prioridade máxima. Lizzie, Ki, Mark e um guarda-base precisam de concepts para sustentar as três fases. Michael, Olive e Brian podem usar retratos narrativos nesta versão. As propostas abaixo evitam ampliar a quantidade de personagens controláveis.

### Prompt E - Lizzie

Concept de Lizzie, amiga adolescente de Magnus, baixa, cabelos castanhos, expressão observadora e corajosa. Proposta de roupa: blusa ciano discreta, jaqueta cinza curta, calça azul-escura e tênis simples. Corpo inteiro em três quartos, pequeno arco de água próximo à mão, postura alerta, fundo marfim. Design juvenil, prático, sem sexualização. Formato vertical 2:3.

### Prompt F - Ki e Mark, gerar individualmente

Ki: jovem instrutor de aparência adolescente, postura disciplinada, roupa de treino marfim com faixa azul-petróleo, cabelo escuro curto, pequena chama flutuante, corpo inteiro e fundo neutro. Mark: antagonista adulto de baixa estatura, expressão controlada e ameaçadora, uniforme cinza-escuro de chefe de segurança com detalhes geométricos laranja discretos, mãos atrás das costas, sem arma em destaque, corpo inteiro e fundo neutro. Os detalhes de roupa são propostas. Gerar uma imagem vertical 2:3 por personagem.

### Prompt G - guarda e variante elite

Prancha de dois guardas de instalação de segurança, corpo inteiro, mesma anatomia-base e uniforme cinza-azulado simplificado. O guarda comum tem equipamento leve e silhueta estreita. A variante elite tem ombreiras compactas e proteção de antebraços mais larga, diferença perceptível pela forma. Rostos simplificados, sem armas realistas detalhadas, sem emblemas de franquias. Fundo marfim e vista de três quartos, formato 3:2.

### Prompt H - aliados narrativos

Gerar retratos individuais consistentes com o estilo: Michael, jovem de pele escura e olhos azuis, postura firme; Olive, jovem de cabelos rosa e olhos amarelos, expressão decidida; Brian, jovem de pele clara, cabelos castanhos e olhos vermelhos, aparência próxima de 18 anos, expressão bem-humorada. Roupas contemporâneas simples azul-petróleo, marfim e detalhes elementais. Fundo liso, busto, formato quadrado. Não juntar rostos na mesma imagem.

### Prompt I - objetos investigáveis

Prancha com quatro objetos separados em vista superior levemente inclinada: pergaminho antigo enrolado, terminal de segurança compacto, cartão de identificação sem texto e painel de porta bloqueada. Formas grandes e simples, contorno azul-petróleo, pequeno destaque marfim para indicar interação. Fundo neutro, nenhum texto legível, formato 1:1. Gerar separadamente depois, caso sejam usados no jogo.

## 05 | Conceitos de cenário

### Regra de câmera e leitura

Concepts panorâmicos comunicam atmosfera. Para construir o jogo, pedir também uma vista superior do piso e dos objetos, com câmera fixa e sem pontos de fuga. Manter os caminhos claros, paredes legíveis e objetos investigáveis destacados. Evitar efeitos de iluminação que escondam a área jogável.

### Prompt J - Acampamento e templo

Cenário de acampamento de treinamento em mata, pequeno templo de madeira com base de pedra, pátio aberto com bonecos de treino, lago raso à esquerda e área segura de prática de fogo à direita. Manhã com luz suave, vegetação verde dessaturada, pedras claras e detalhes laranja e ciano. Entrada do templo claramente visível, caminho largo conectando pátio e saída para a floresta, sem personagens. Composição para exploração de RPG 2D, vista superior levemente inclinada, sem isometria, formato 16:9.

### Prompt K - Posto DASA

Interior compacto de empresa de segurança que esconde operações da ASDA. Recepção simples, corredor central, arquivo lateral com documentos e sala de segurança com terminal. Piso cinza claro, paredes azul-acinzentadas, luz fria e pequenos indicadores ciano. Ambiente aparentemente organizado, com detalhes que sugerem segredo. Caminho principal livre, portas e salas distinguíveis, sem pessoas e sem texto gerado, câmera superior levemente inclinada, formato 16:9.

### Prompt L - Base ASDA

Base inimiga de segurança: entrada estreita desemboca em corredor com painéis, antecâmara e sala final ampla com plataforma central. Metal cinza-escuro, sombras azul-petróleo, pequenos sinais laranja e núcleo de energia ciano ao fundo como elemento cenográfico. Clima de confronto e tensão, sem gore, sem explosão que esconda o piso. Área central livre para composição de batalha e rota legível até o chefe, sem personagens, câmera superior levemente inclinada, formato 16:9.

### Fundos de combate

Para cada cenário aprovado, pedir uma variante lateral em 16:9: herói ocupará o terço esquerdo e inimigo o direito, com chão horizontal comum; deixar o quarto inferior sem detalhes importantes para o menu de ações. Usar o mesmo material, paleta e iluminação do respectivo mapa. Não incluir combatentes no fundo.

### Entrega visual esperada

Três concepts de exploração e três fundos de batalha derivados. Reutilizar materiais e objetos entre DASA e ASDA. O cenário conceitual não precisa ser um mapa navegável; a topologia obrigatória está no planejamento de fases a seguir.

## 06 | Mapas e progressão

### Convenção para os sketches

Mapas vistos de cima, com caixas para salas e linhas para passagens. A entrada e a saída devem ser distintas. B significa batalha obrigatória; P significa pista obrigatória. Os diagramas gerados são referências: adicionar rótulos manualmente e conferir as conexões. Dimensões abaixo são propostas em tiles de 32 x 32 px.

### Fase 1 - aprender | 32 x 24 tiles

Fluxo: entrada > pátio com B1 treino > templo com P1 traição > trilha com B2 guarda > saída. A ligação para a trilha fica bloqueada até descobrir P1. Lago e bonecos são cenário, sem minigame adicional. Ensina movimento, interação, ataque, defesa e recompensa. Estimativa de 6 a 8 minutos. Magnus começa no nível 1 e deve chegar ao nível 2 ao terminar esta área; validar o total de XP no protótipo.

### Prompt M - sketch da fase 1

Planta esquemática superior de mapa pequeno: entrada no sul, grande pátio central, lago decorativo no oeste, templo no norte, trilha de fuga saindo pelo leste do pátio. Conectar entrada ao pátio, pátio ao templo e pátio à trilha. Marcar dois círculos vazios para encontros, um no pátio e um na trilha; um losango vazio dentro do templo para pista. Fundo branco, linhas escuras, sem texto, sem perspectiva, formato 4:3.

### Fase 2 - investigar | 36 x 24 tiles

Fluxo: recepção > B3 guarda no corredor > arquivo com P2 identificação > B4 guarda na sala de segurança > terminal com P3 localização da base > saída. A saída exige P2, P3 e ambas as vitórias; manter retorno livre às pistas para evitar bloqueio. Ensina consultar objetivos e alternar dano com proteção. Estimativa de 7 a 10 minutos; planejar nível 3 ao final.

### Prompt N - sketch da fase 2

Planta superior de instalação retangular pequena: recepção no sul, corredor central, arquivo a oeste, sala de segurança a leste e saída ao norte. Corredor conecta todas as salas. Dois círculos vazios, um no corredor e um na sala de segurança; dois losangos vazios, um no arquivo e outro junto ao terminal da sala de segurança. Fundo branco, linhas escuras, sem texto, formato 3:2.

### Fase 3 - combinar | 32 x 28 tiles

Fluxo: entrada > B5 elite > antecâmara > B6 Mark > desfecho. Recuperar recursos na antecâmara para o chefe não depender de itens ou grind. O elite exige uso da defesa; Mark alterna ataque simples e preparação visível de ataque forte, dando um turno para reagir. Estimativa de 7 a 12 minutos. Ao vencer, reproduzir o desfecho com Brian e a queda da ASDA por diálogos.

### Prompt O - sketch da fase 3

Planta superior de base compacta: entrada sul, corredor reto, antecâmara intermediária e arena retangular ampla ao norte; conexões lineares e claras. Um círculo vazio no corredor para elite e um círculo maior no centro da arena para chefe. Fundo branco, linhas escuras, sem texto, formato 4:3.

## 07 | Sprites e animações

### Padrão técnico proposto

Tiles de 32 x 32 px; sprite de exploração de Magnus em células de 64 x 64 px, quatro direções. Sprites de combate em células de 256 x 256 px, vista lateral. PNG com transparência real, escala constante, pivô dos personagens no centro dos pés. Esses tamanhos são alvos de exportação após edição, não garantia de precisão da ferramenta generativa.

### Magnus - exploração | 20 quadros

Parado: 1 quadro por direção, total 4. Andar: 4 quadros por direção, total 16, repetição a cerca de 8 quadros por segundo. As direções são frente, costas, esquerda e direita. Mesmo chão de apoio e mesma altura em todas as células. O ciclo deve começar e terminar sem salto de posição.

### Magnus - combate | 20 quadros

Parado: 2 quadros em repetição; ataque básico: 4; conjurar elemento: 4, reutilizados para fogo e água com efeitos separados; defender: 2; receber dano: 2; derrota: 4; vitória: 2. Ações entre 8 e 12 quadros por segundo como ponto inicial. Defesa pode manter o último quadro até receber o golpe.

### NPCs e inimigos

Lizzie, Ki, Michael, Olive e Brian: um sprite estático de exploração e um retrato cada. Guarda comum, elite, Ki de treino e Mark: por conjunto de combate, 2 quadros parado, 4 atacando, 2 recebendo dano e 4 de derrota, total 12. Guarda comum e elite reutilizam animação-base. A derrota do treino de Ki é uma pose de encerramento, não morte.

### Efeitos e interface

Quatro efeitos separados: impacto básico com 4 quadros, fogo com 6, água com 6 e defesa com 4, total 20. Planejar tiles compartilhados de chão, parede e cantos, quatro objetos investigáveis, ícones de vida, energia, XP, fogo, água e defesa. Texto de interface deve ser inserido na Unity, não desenhado dentro das imagens.

### Prompt P - referência de movimento

Usar Magnus aprovado como referência. Criar quatro poses sequenciais do mesmo ciclo de caminhada visto de frente, corpo inteiro, mesma altura e pés na mesma linha de base, igualmente espaçadas em uma linha. Roupa e rosto invariáveis, fundo liso contrastante, sem chão, sem sombra projetada e sem texto. Esta é uma referência para reconstrução de sprites. Repetir separadamente para costas, esquerda e direita.

### Prompt Q - efeito elemental

Criar estudo de seis momentos de uma pequena bola de fogo estilizada, da formação ao impacto, formas simples laranja e marfim com contorno discreto, quadros separados sem sobreposição, fundo uniforme. Repetir em outra imagem com jato de água ciano em arco. Sem personagem e sem texto. Recortar, limpar o fundo e alinhar os quadros manualmente antes de importar.

## 08 | Organização e atualização do GDD

### Ordem de geração recomendada

1. Aprovar o concept de Magnus. 2. Gerar suas vistas e expressões com referência. 3. Gerar Lizzie, Ki, Mark e guarda-base. 4. Gerar cenários e fundos. 5. Gerar sketches de fases e conferir rotas. 6. Produzir poses e efeitos para orientar sprites. 7. Montar as pranchas com títulos fora da ferramenta de geração.

### Arquivos e nomes

Salvar concepts em Art/Concepts, pranchas em Art/ModelSheets, estudos em Art/AnimationStudies e mapas em LevelDesign. Exemplos: magnus_concept_v01.png, magnus_modelsheet_v01.png, fase01_acampamento_sketch_v01.png. Quando houver projeto Unity, importar assets finais em Assets/Art e versionar os respectivos arquivos .meta. Separar imagem de referência de sprite pronto para uso.

### Checklist de revisão

Conferir rosto, idade, roupa e paleta; ausência de membros extras; vistas coerentes; silhueta legível em tamanho reduzido; nenhum texto aleatório; caminhos conectados; espaço para menus nos fundos de batalha. Para sprites, conferir tamanho das células, transparência, alinhamento dos pés e continuidade do ciclo. Não considerar uma folha gerada pronta sem edição e teste.

### Mudanças registradas no GDD v0.2

Definidos estilo ilustrado 2D, paleta, design proposto de Magnus, especificações de sprites, inventário de animação, identidade dos três cenários, fluxo das fases e progressão dos desafios. Mantidos combate por turnos, investigação, XP, três mapas, seis encontros e final inspirado no livro. Os valores de balanceamento e o resultado visual dependem de prototipagem e aprovação da equipe.

### O que esta versão entrega

Guia visual em PDF e Markdown, prompts para outra ferramenta, planejamento de fases e GDD v0.2 atualizado. A tag v0.2 identifica esta revisão de planejamento. Nenhuma arte, model sheet ou animação final foi gerada neste pacote, por escolha do fluxo solicitado. Inserir as imagens produzidas no mesmo repositório e criar um novo marco, como v0.2.1, após revisão, preservando o histórico.

### Pendências para apresentar a etapa visual

Gerar e selecionar as imagens; diagramar os model sheets e mapas; revisar com a equipe; acrescentar créditos e origem das imagens usadas; publicar o repositório remoto, que ainda não foi criado. O guia de prompts prepara a produção, mas as imagens previstas na lista da entrega ainda precisam ser anexadas.
