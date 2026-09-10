from pathlib import Path
from xml.sax.saxutils import escape
from reportlab.pdfgen import canvas
from reportlab.platypus import SimpleDocTemplate, Paragraph, Spacer, PageBreak
from reportlab.lib.styles import ParagraphStyle
from reportlab.lib.colors import HexColor
from reportlab.lib.enums import TA_LEFT

ROOT = Path(__file__).resolve().parents[1]
PAGES = [
('Concepção do jogo', [
('Identificação', 'Nome provisório: Between fire and water. Gênero: RPG 2D por turnos, para um jogador. Tecnologia prevista: Unity e C#. Público-alvo: pessoas a partir de 13 anos interessadas em fantasia, investigação e evolução de personagem. Plataformas-alvo: PC e mobile; Windows e Android são a proposta inicial de sistemas.'),
('Entrega e versão', 'Entrega 1 - Concepção e GDD Inicial. Data prevista na disciplina: 03/09/2026. Documento preparado em 10/09/2026 para a entrega pendente. Versão documental: v0.1. Esta versão registra planejamento; não representa um protótipo implementado.'),
('High Concept', 'Um jovem capaz de controlar fogo e água descobre que a agência que prometeu protegê-lo quer eliminá-lo. Em um RPG 2D por turnos baseado em Entre fogo e água, o jogador acompanha Magnus Lubros, investiga pistas sobre a ASDA, domina habilidades elementais e fortalece-se em batalhas até enfrentar a organização e descobrir quem realmente cumpre a profecia.'),
('Objetivo do jogador', 'Concluir o modo história: investigar a ameaça, superar os confrontos obrigatórios e chegar ao desfecho da queda da ASDA. A evolução por experiência permite enfrentar desafios progressivamente mais fortes.'),
('Pilares da experiência', 'Combate acessível: decisões claras em turnos. Descoberta: examinar o mapa revela pistas e desbloqueia objetivos. Progressão: vencer confrontos concede XP e aumenta o poder de Magnus. Narrativa: a campanha mantém o conflito e a revelação central do livro.'),
('Status das decisões', 'Nome, gênero, integrantes, público, plataformas, objetivo, combate, exploração e progressão foram informados pelo grupo. Divisão de papéis, quantidade de conteúdo, controles e regras numéricas deste GDD são propostas iniciais a validar e ajustar durante o desenvolvimento.')]),
('Equipe e produção', [
('Larissa Campos Cardoso', 'Proposta de responsabilidade principal: Game Designer + Producer. Organizar conceito, roteiro adaptado, GDD, escopo, cronograma e acompanhamento das entregas. Apoiar testes de compreensão da história e dos objetivos.'),
('Izak Pinheiro', 'Proposta de responsabilidade principal: Artista 2D + Animador. Produzir concepts, model sheets, sprites, cenários, interface e animações. Acumular Level Design + Sound Design: planejar os três mapas e selecionar, produzir ou editar os efeitos e a trilha.'),
('Matheus Henrique', 'Proposta de responsabilidade principal: Programador + QA. Implementar exploração, combate, XP, interface, salvamento e integração na Unity. Gerar builds, registrar bugs e verificar controles, progressão e estabilidade em PC e mobile.'),
('Trabalho compartilhado', 'A equipe tem três integrantes; a divisão acima acumula áreas para cobrir a produção. Todos participam de playtests e revisões. A atribuição nominal ainda depende da validação do grupo e não comprova contribuições já realizadas. Cada integrante deverá registrar suas contribuições reais no histórico do projeto.'),
('Fluxo de trabalho proposto', 'Manter um único repositório com documentos, código e materiais produzidos. Usar tarefas pequenas com responsável e critério de conclusão. Atualizar o GDD quando houver mudança de regra ou escopo. Realizar uma revisão da versão antes de cada entrega e marcar a revisão correspondente com uma tag.'),
('Cronograma da disciplina', '03/09 - v0.1: concepção e GDD inicial (entrega pendente). 10/09 - v0.2: direção visual e planejamento de fases. 17/09 - v0.3: protótipo jogável. 24/09 - v0.4: vertical slice com sistemas integrados. 01/10 - v0.5: conteúdo completo, playtesting e correções. 08/10 - v1.0: build final, GDD final e apresentação.'),
('Prioridades imediatas', 'Validar os papéis e este escopo; entregar a documentação v0.1; produzir os materiais visuais e mapas da v0.2; iniciar na Unity uma cena de exploração conectada a uma batalha. A concepção deve orientar a implementação sem exigir que o jogo completo exista nesta entrega.')]),
('História e adaptação', [
('Referência principal', 'Entre fogo e água, manuscrito fornecido pelo grupo em entre-fogo-e-agua.pdf, 21 páginas. Autoria e edição não foram informadas no pedido. As referências abaixo usam a numeração das páginas do arquivo PDF. O livro é fonte narrativa; as regras de jogo são propostas de adaptação.'),
('Mundo e conflito - páginas 1 a 6', 'O prólogo apresenta uma profecia sobre alguém ligado ao fogo e à água e uma organização criada para impedir sua realização. Na trama contemporânea, Magnus Lubros, de 14 anos, possui ligação com as duas nações. Ele e Lizzie entram em contato com a ASDA, que se apresenta como uma agência de proteção.'),
('Treinamento e traição - páginas 7 a 16', 'Magnus treina com Ki e aprende a controlar seus poderes. A confiança nos responsáveis pelo acampamento se desfaz quando a intenção de eliminá-lo é revelada. Ele foge, é capturado e escapa novamente com Lizzie, que também demonstra poderes.'),
('Aliados e investigação - páginas 16 a 19', 'Magnus e Lizzie encontram Michael, Olive e Brian. O grupo investiga a ASDA e identifica a ligação da empresa DASA com a organização. Esses acontecimentos fundamentam os objetivos de investigação e a progressão até a base inimiga.'),
('Desfecho - páginas 20 e 21 | spoilers', 'No confronto final, Brian se sacrifica e cria a oportunidade para os amigos reagirem e destruírem a ASDA. A revelação é que Brian, e não Magnus, cumpre a profecia em seu aniversário de 18 anos. O epílogo mostra os sobreviventes procurados após a destruição da empresa.'),
('Recorte para o jogo', 'Proposta: condensar a campanha em treinamento, investigação e confronto final. O prólogo e as viagens serão apresentados em diálogos curtos. Magnus será o único personagem controlável; Lizzie, Michael, Olive e Brian participarão por diálogos e eventos. Mark será adaptado como chefe final. O combate vencido conduz ao desfecho narrativo inspirado no livro, preservando a revelação sobre Brian.'),
('Tom e consistência', 'Adotar aventura e tensão com violência estilizada, sem gore e sem conteúdo sexual. Romance apenas sugerido. A faixa 13+ é uma intenção de público, não uma classificação oficial. Padronizar no roteiro os nomes usados de forma inconsistente no manuscrito e a idade de Brian conforme a revelação final. Usar áudio original ou com licença adequada; músicas citadas no livro não integram o escopo sonoro automaticamente.')]),
('Mecânicas e core loop', [
('Mecânica principal: batalhas em turnos', 'Proposta inicial: Magnus enfrenta um inimigo por encontro. O jogador escolhe uma ação por turno: ataque básico, habilidade de fogo, habilidade de água ou defesa. Após a ação e a verificação de vida, o inimigo age se estiver vivo. A sequência se repete até a vitória ou derrota. Menus de ação pausam a decisão, sem limite de tempo.'),
('Fogo, água e defesa', 'O ataque básico não consome energia. A habilidade de fogo causa dano maior; a de água causa dano moderado e reduz o próximo dano recebido; defender reduz o próximo dano e recupera energia. Cada efeito defensivo expira após o próximo ataque recebido e não acumula. Valores, custos e multiplicadores serão definidos no protótipo. A interface deve mostrar custos, vida, energia e efeitos ativos.'),
('Mecânica secundária: investigação de mapa', 'Explorar mapas 2D com visão superior, conversar com NPCs e examinar objetos destacados. Pistas obrigatórias atualizam o objetivo e liberam a próxima área. Pistas opcionais acrescentam contexto. A aproximação de um inimigo visível inicia o combate; encontros obrigatórios vencidos não reaparecem.'),
('Experiência e evolução', 'Derrotar inimigos concede XP uma única vez por encontro. Atingir os limiares de XP aumenta o nível e melhora vida e poder de ataque. Proposta de limite: três níveis na campanha curta. O total de XP dos encontros obrigatórios deve permitir concluir o jogo sem repetir batalhas. Não haverá perda de nível após derrota.'),
('Core loop', 'Explorar o mapa > investigar pistas e identificar o objetivo > enfrentar e derrotar inimigos > receber XP > subir de nível e ficar mais forte > desbloquear a próxima etapa da história > explorar a nova área. O ciclo termina ao concluir o confronto final e o epílogo.'),
('Vitória e derrota', 'Na batalha, vence quem reduzir primeiro a vida do oponente a zero. A vida é limitada a zero no mínimo e a resolução interrompe as ações seguintes; não há ações simultâneas nesta proposta. Se Magnus chegar a zero, mostrar derrota e permitir reiniciar o encontro no estado anterior, sem duplicar XP. A vitória da campanha exige terminar os objetivos obrigatórios, vencer o chefe e assistir ao desfecho.'),
('Sistemas de apoio', 'Prever pausa, volume, retorno ao menu e um salvamento automático local ao concluir cada área. Ao carregar, restaurar nível, XP e objetivos concluídos. A cena narrativa de sacrifício de Brian não será tratada como derrota do jogador.')]),
('Escopo inicial e experiência', [
('Campanha proposta', 'Duração estimada: 20 a 30 minutos, a confirmar em playtest. Três mapas pequenos, seis encontros no total e um único final. Uma campanha linear completa com começo, desenvolvimento e conclusão, condensando o livro para o prazo disponível.'),
('Mapa 1 - Acampamento e templo', 'Apresentar Magnus, Lizzie, Ki e o treinamento. Ensinar movimento, interação, ataque e defesa com um encontro de treino. A descoberta da traição abre uma rota de fuga com um guarda. Saída: alcançar o ponto de fuga após cumprir os objetivos. Total: dois encontros.'),
('Mapa 2 - Posto DASA', 'Apresentar os aliados por diálogos e investigar o vínculo da empresa com a ASDA. Encontrar duas pistas obrigatórias: identificação da organização e registro da base. Enfrentar dois guardas e liberar o caminho para o confronto final. Total: dois encontros.'),
('Mapa 3 - Base ASDA', 'Percorrer uma área curta até Mark. Vencer um guarda de elite e o chefe final; apresentar o sacrifício de Brian, a queda da ASDA e o epílogo em sequência narrativa. Total: dois encontros. O chefe é uma adaptação de gameplay, não uma descrição literal do combate do livro.'),
('Conteúdo e recursos previstos', 'Um personagem controlável; quatro aliados narrativos; Ki como instrutor e oponente de treino; Mark como antagonista. Reutilizar a base visual de guarda em variantes comum e elite. Prever sprites de exploração de Magnus, NPCs estáticos, retratos essenciais, poses de combate e efeitos simples de fogo e água. Animações mínimas: parado, andar, atacar, receber dano e derrota.'),
('Interface e controles', 'PC: WASD ou setas para mover, E para interagir, mouse para menus e Esc para pausar. Mobile: direcional virtual, botão de interação e seleção de ações por toque, em orientação horizontal. Prever menu inicial, objetivo atual, HUD de vida/energia/nível, menu de combate, vitória, derrota e créditos. Usar texto e ícones junto às cores de fogo e água.'),
('Limites de produção', 'Ficam fora da v1.0 proposta: mundo aberto, multiplayer, combate com equipe controlável, criação de itens, árvore extensa de habilidades, finais alternativos, dublagem e adaptação de todas as cenas do livro. PC e mobile continuam como alvos; validar Android cedo e priorizar a estabilização do PC. Se o prazo apertar, reduzir diálogos e variedade visual antes de remover história, investigação ou turnos.')]),
('Validação e entrega v0.1', [
('Critérios para o protótipo v0.3', 'Mover Magnus com colisões; interagir com uma pista; iniciar e concluir uma batalha; impedir ações fora do turno; encerrar imediatamente quando a vida chegar a zero; conceder XP uma única vez; voltar ao mapa após vitória e reiniciar após derrota. Verificar também o toque em um aparelho Android disponível.'),
('Critérios para a versão final', 'Concluir os três mapas sem bloqueios; identificar o objetivo atual; alcançar o chefe com o XP dos encontros obrigatórios; terminar a história; salvar e retomar a progressão sem duplicar recompensas. Conferir legibilidade, controles, áudio e estabilidade em Windows e Android. Registrar aparelho, sistema, versão do jogo e resultado de cada teste.'),
('Playtesting previsto', 'Na beta, realizar sessões com pessoas externas ao desenvolvimento, registrar tempo de conclusão, pontos de dúvida e derrotas. Classificar bugs por impacto, corrigir primeiro travamentos e bloqueios de progressão e repetir os percursos afetados. Ainda não há playtests ou resultados nesta versão de concepção.'),
('Riscos e resposta', 'Escopo narrativo: condensar eventos nos três mapas. Sobrecarga de arte e level design: reutilizar tiles e variantes de guardas e redistribuir apoio entre integrantes. Mobile: testar resolução e toque desde o protótipo. Balanceamento: evitar dependência de repetição de encontros. Divergências do manuscrito: manter as decisões de adaptação registradas no GDD.'),
('Conteúdo desta entrega', 'GDD inicial editável e em PDF; README de apresentação; planejamento e critérios de validação; referência narrativa identificada; repositório Git local com marco v0.1 após a revisão dos arquivos. Esta entrega não inclui build nem projeto Unity implementado. O projeto Unity e os assets produzidos devem entrar neste mesmo repositório nas próximas etapas.'),
('Versionamento e envio', 'A tag v0.1 deve apontar para o commit da concepção. A disponibilização ao professor requer um endereço remoto acessível contendo esse commit e a tag. O endereço remoto depende do repositório informado pelo grupo; a tag local, sozinha, não disponibiliza o material online. Não alterar retroativamente a data real dos commits para simular a entrega de 03/09.'),
('Pendências da equipe', 'Validar os papéis propostos; confirmar o recorte e os alvos Windows/Android; informar autoria do livro para os créditos; informar ou criar o repositório remoto e disponibilizar a v0.1. O GDD é um documento vivo e deverá registrar as próximas alterações.'),
('Histórico', 'v0.1 - 10/09/2026: concepção inicial preparada a partir do pedido do grupo e do manuscrito, referente à etapa prevista para 03/09/2026.')]),
]

def main():
    docs = ROOT / 'docs'
    out = ROOT / 'output' / 'pdf'
    docs.mkdir(exist_ok=True)
    out.mkdir(parents=True, exist_ok=True)
    md = ['# Between fire and water', '', '## GDD inicial | v0.1', '']
    for title, sections in PAGES:
        md += ['## ' + title, '']
        for heading, body in sections:
            md += ['### ' + heading, '', body, '']
    (docs / 'GDD-v0.1.md').write_text('\n'.join(md), encoding='utf-8')
    styles = {
        'title': ParagraphStyle('title', fontName='Helvetica-Bold', fontSize=24, leading=29, textColor=HexColor('#152C3D'), spaceAfter=17),
        'heading': ParagraphStyle('heading', fontName='Helvetica-Bold', fontSize=11, leading=14, textColor=HexColor('#A64B24'), spaceBefore=10, spaceAfter=4),
        'body': ParagraphStyle('body', fontName='Helvetica', fontSize=10, leading=14, textColor=HexColor('#243746'), spaceAfter=4),
    }
    def frame(c, doc):
        w,h=doc.pagesize
        c.setFillColor(HexColor('#152C3D')); c.rect(0,h-46,w,46,fill=1,stroke=0)
        c.setFont('Helvetica-Bold',10); c.setFillColor(HexColor('#FFFFFF')); c.drawString(42,h-28,'BETWEEN FIRE AND WATER')
        c.setFillColor(HexColor('#E17A3C')); c.rect(42,h-55,70,3,fill=1,stroke=0)
        c.setFillColor(HexColor('#259EBC')); c.rect(112,h-55,70,3,fill=1,stroke=0)
        c.setFont('Helvetica',8); c.setFillColor(HexColor('#526878'))
        c.drawString(42,25,'GDD inicial | v0.1 | Concepção | 10/09/2026')
        c.drawRightString(w-42,25,str(doc.page))
    flow=[]
    for i,(title,sections) in enumerate(PAGES):
        if i: flow.append(PageBreak())
        flow.append(Paragraph(escape(title),styles['title']))
        for heading,body in sections:
            flow.append(Paragraph(escape(heading),styles['heading']))
            flow.append(Paragraph(escape(body),styles['body']))
    target=out/'Between-fire-and-water-GDD-v0.1.pdf'
    SimpleDocTemplate(str(target),pagesize=(595.28,841.89),rightMargin=42,leftMargin=42,topMargin=75,bottomMargin=47,title='Between fire and water - GDD inicial v0.1',author='Equipe Between fire and water').build(flow,onFirstPage=frame,onLaterPages=frame)
    print(target)

if __name__ == '__main__': main()
