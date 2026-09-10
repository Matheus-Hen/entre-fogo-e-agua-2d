"""Gera o GDD v0.2 em Markdown e PDF; preserva os arquivos da v0.1."""
from pathlib import Path
from copy import deepcopy
from xml.sax.saxutils import escape
import json
from PIL import Image as PILImage
from reportlab.platypus import SimpleDocTemplate, Paragraph, Spacer, PageBreak, Image
from reportlab.lib.styles import ParagraphStyle
from reportlab.lib.colors import HexColor
from gerar_gdd import PAGES, ROOT

# Os caminhos são relativos ao repositório e usados também no catálogo.
ASSETS = [
 ('image-gen-1(3).png', '02-model-sheet/magnus-frente.png', 'Magnus - vista frontal; quatro poses de caminhada'),
 ('image-gen-2(2).png', '02-model-sheet/magnus-costas.png', 'Magnus - vista posterior; quatro poses de caminhada'),
 ('image-gen-3(2).png', '02-model-sheet/magnus-direita.png', 'Magnus - perfil direito; quatro poses de caminhada'),
 ('image-gen-4(1).png', '02-model-sheet/magnus-esquerda.png', 'Magnus - perfil esquerdo; quatro poses de caminhada'),
 ('image-gen-1(4).png', '01-concept-art/efeito-fogo.png', 'Concept de fogo - formação, deslocamento e impacto'),
 ('image-gen-2(3).png', '01-concept-art/efeito-agua.png', 'Concept de água - formação, arco e impacto'),
 ('Arena do Reator Cibernético.png', '04-cenarios/base-asda-arena-reator.png', 'Conceito de cenário - arena final da ASDA'),
 ('Mapa Esquemático de Templo e Masmorras.png', '05-level-design/mapa-01-acampamento-templo.png', 'Mapa 1 - acampamento e templo'),
 ('Planta Arquitetônica em Preto e Branco.png', '05-level-design/mapa-02-posto-dasa.png', 'Mapa 2 - posto DASA'),
 ('Planta Esquemática em Preto e Branco.png', '05-level-design/mapa-03-base-asda.png', 'Mapa 3 - base ASDA'),
]

def sections():
    pages = deepcopy(PAGES[:5])
    replacements = {
      'Entrega e versão': 'Entrega 2 - Direção Visual e Level Design. Data: 10/09/2026. Valor: 1,0 ponto. Versão: v0.2. Atualiza o GDD v0.1 com as dez imagens fornecidas, direção de arte, planejamento de sprites, cenários, fases e desafios. Documento de pré-produção; ainda não há build ou projeto Unity.',
      'Prioridades imediatas': 'Validar a direção visual e as responsabilidades; preparar recorte e padronização dos sprites; implementar na v0.3 o movimento, uma pista e uma batalha. Os mapas e parâmetros desta versão são propostas para validação no protótipo.',
      'Referência principal': 'Base desta revisão: Between-fire-and-water-GDD-v0.1.pdf, seis páginas, preservado em output/pdf. O GDD anterior identifica o manuscrito Entre fogo e água, 21 páginas, como fonte narrativa. As referências de páginas do livro abaixo são herdadas da v0.1; o manuscrito não foi consultado novamente nesta revisão. Autoria e edição continuam pendentes.',
    }
    for _, items in pages:
        for i,(h,b) in enumerate(items):
            if h in replacements: items[i]=(h,replacements[h])
    pages += [
     ('Direção visual e referências', [
      ('Identidade proposta', 'Fantasia contemporânea com personagem de traço ilustrado/anime, roupas cotidianas e poderes elementais. A ASDA usa arquitetura industrial, metal escuro, tubulações e luz ciano. A oposição entre fogo e água deve guiar efeitos e interface sem transformar o jogo em plataforma: permanece RPG de exploração superior e combate por turnos.'),
      ('Paleta de produção', 'Fogo: laranja #E17A3C e amarelo #FFD166. Água: ciano #259EBC e azul-claro #7BDFF2. Estruturas e interface: azul-escuro #152C3D e cinza #526878. Texto claro: #F4F1E8. São cores propostas para padronização, não uma extração exata dos pixels das imagens.'),
      ('Forma, leitura e câmera', 'Magnus deve manter silhueta reconhecível, cabelo castanho, jaqueta azul-petróleo, camiseta clara, calça cinza e tênis claros. Fogo usa pontas e expansão; água usa curvas e gotas. Exploração com câmera superior levemente inclinada; as vistas atuais do personagem precisam ser adaptadas a esse ângulo. Arena frontal reservada à cena de combate.'),
      ('Referências disponíveis', 'As dez imagens enviadas são o painel visual interno desta entrega: quatro pranchas de Magnus, dois efeitos elementais, uma arena e três plantas. Não foram atribuídos artistas, licenças ou jogos de referência sem fonte. A equipe deve confirmar autoria, ferramenta e permissão de uso antes dos créditos finais. Os nomes originais e hashes estão em imagens/inventario.json.'),
      ('Interface e acessibilidade', 'Fonte sem serifa, alto contraste, painéis escuros e textos claros. Ações identificadas por nome e ícone: chama, gota e escudo; não depender apenas da cor. Manter objetivo visível e indicação de interação. Proposta mobile: alvos de toque de pelo menos 48 unidades lógicas, a testar no aparelho escolhido.'),
     ]),
     ('Magnus - model sheet de referência', [
      ('Prancha e proporções', 'As quatro vistas nas páginas seguintes formam o model sheet preliminar de Magnus. Cada tira apresenta quatro poses; serve para consultar frente, costas e os dois perfis e estudar a caminhada. Não é um turnaround neutro final: antes da produção, alinhar altura, linha dos pés, volume do cabelo, mangas e proporções entre vistas.'),
      ('Regras de consistência', 'Preservar cores da roupa, desenho da jaqueta e silhueta em todas as direções. Usar um único pivô nos pés. A altura da cabeça e o comprimento das pernas não devem oscilar entre frames. Expressão inicial determinada; dano e derrota precisam de poses próprias. A aparente idade deve respeitar Magnus, 14 anos, conforme a narrativa da v0.1.'),
      ('Status de produção', 'Disponível: referência visual do protagonista e poses de locomoção em quatro direções. A produzir: poses neutras alinhadas, ataque, conjuração, defesa, dano, derrota e expressões. Ki, aliados, guardas e Mark ainda precisam de concepts próprios; reutilizar a base de guarda para elite com insígnia e silhueta distinguíveis.'),
     ]),
     ('Sprites e animações - plano técnico', [
      ('Formato proposto', 'Arte 2D ilustrada com transparência PNG/RGBA. Unidade inicial: tiles de 64 x 64 px, 64 pixels por unidade; personagem em célula de 128 x 192 px com pivô inferior central. Esses números são alvos de produção a validar em tela, não dimensões dos PNGs recebidos. Separar colisão dos pés do volume visual.'),
      ('Magnus na exploração', 'Idle: 1 pose por direção, 4 frames totais. Caminhada: 4 frames por direção, 16 no total, loop a 8 fps. As quatro tiras de 2172 x 724 px são referências para esses ciclos; recorte automático em quatro partes não garante pivôs nem margens consistentes. Preferir animação e recorte manual com alinhamento.'),
      ('Magnus no combate', 'Plano mínimo para uma orientação: ataque básico 4 frames/10 fps; fogo 4/10 fps; água 4/10 fps; defesa 2/6 fps; dano 2/8 fps; derrota 4/6 fps. Total adicional: 20 frames. Disparar dano/efeito em evento definido da animação; ações continuam regidas pelo turno. Não repetir automaticamente ataque ou derrota.'),
      ('Efeitos e demais personagens', 'Fogo: 6 frames alvo/12 fps; água: 6/12 fps, sem loop. Os concepts mostram estágios visuais, não uma sprite sheet pronta. NPCs narrativos: uma pose inicial para Lizzie, Michael, Olive e Brian. Ki, guarda, elite e Mark: idle, ataque, dano e derrota, com reutilização para reduzir trabalho. Validar antes de ampliar o inventário.'),
      ('Preparação e importação', 'Remover fundos opacos, normalizar escala e margens, corrigir inconsistências, exportar sem halo e manter originais conceituais. Na Unity, importar como Sprite, usar Multiple para atlas, pivô nos pés e filtro Bilinear para esta proposta ilustrada. Nome: magnus_walk_front_00.png; efeitos: fx_fire_00.png. Guardar assets finais e .meta juntos no futuro projeto Unity.'),
      ('Aceite visual', 'Testar ciclos sobre fundo claro e escuro: sem saltos de pivô, halos ou mudança de tamanho. Confirmar leitura em Windows e Android, custos das ações e contraste. As imagens atuais não comprovam animações implementadas nem integração na engine.'),
     ]),
     ('Cenários - linguagem por fase', [
      ('Mapa 1: acampamento e templo', 'Exterior de terra e vegetação, pedra envelhecida no templo e acentos quentes no treino. Usar trilhas largas e marcos distintos para templo ao norte e fuga a leste. A planta existente define a composição espacial; pintura de cenário, tiles de chão, paredes, árvore, altar e saída ainda serão produzidos.'),
      ('Mapa 2: posto DASA', 'Recepção clara e salas administrativas frias com mesas, arquivos e terminais. Os objetos das duas pistas devem se destacar por ícone e interação, evitando que a leitura dependa de detalhes pequenos. A planta serve de conceito espacial; ainda faltam tiles e ilustração final.'),
      ('Mapa 3: base ASDA', 'Corredor de acesso estreito leva a uma arena ampla. A imagem do reator define metal escuro, luzes ciano e pontos quentes de alerta. O reator é ambientação proposta, não novo sistema de combate. O fundo frontal será usado na batalha; produzir tiles compatíveis com a exploração superior.'),
      ('Camadas e colisão', 'Separar piso, obstáculos, personagens e elementos acima do jogador. Paredes e máquinas bloqueiam; luz, fumaça e efeitos decorativos não bloqueiam. Corredores com pelo menos duas células livres e portas legíveis. Nada no cenário deve esconder pistas obrigatórias ou os controles de toque.'),
     ]),
     ('Level design - regras de leitura', [
      ('Convenção das plantas', 'Nas plantas fornecidas, círculos serão pontos de encontro; losangos, objetos de investigação; portas e passagens representam conexões. A associação é uma decisão desta revisão. Os detalhes de cada página definem função e condição de acesso. Formas irregulares do mapa 1 são terreno e vegetação, não novos encontros.'),
      ('Escala e fluxo', 'Mapas esquemáticos, sem escala métrica original. Proposta para blockout: mapa 1 em 32 x 24 tiles, mapa 2 em 24 x 18 e mapa 3 em 20 x 28. Usar tiles de 64 px. Ajustar dimensões após testar movimento e câmera, mantendo a topologia e os seis encontros previstos.'),
      ('Sinalização e recuperação', 'Portas bloqueadas informam a condição faltante. Pista opcional nunca impede progresso. Derrota retorna ao estado anterior ao encontro; vitória marca o encontro como concluído e impede duplicar XP. Salvamento automático ao concluir cada mapa. Um objetivo textual orienta o próximo passo.'),
      ('Critérios de blockout', 'Percorrer entrada até saída com colisões; tentar sair antes de cumprir cada condição; visitar as salas do mapa 2 em ordens diferentes; reiniciar após derrota e recarregar após transição. Verificar que nenhuma pista fica inacessível e que não há encontros extras implícitos nos desenhos.'),
     ]),
     ('Progressão dos desafios', [
      ('Mapa 1 - aprender | 6 a 8 minutos', 'E1: Ki no pátio central ensina ataque e defesa com instruções visíveis. Após o treino, investigar o altar ao norte revela a traição. E2: guarda na passagem leste exige escolher entre dano de fogo e proteção de água. Vencer libera a fuga. Objetivo de aprendizagem: compreender exploração, interação e turno.'),
      ('Mapa 2 - aplicar | 7 a 10 minutos', 'E3: guarda no corredor protege acesso às salas. Investigar a identificação no arquivo oeste e o registro da base na sala leste. E4: guarda na sala leste testa gestão de energia; a pista dessa sala fica acessível após a vitória. A porta norte exige as duas pistas e ambos os encontros vencidos. Objetivo: combinar investigação e combate sem repetição de batalhas.'),
      ('Mapa 3 - dominar | 7 a 12 minutos', 'E5: elite no corredor prepara o uso de defesa contra ataque forte anunciado. E6: Mark na arena alterna ataque comum e preparação de ataque forte, com aviso textual. O jogador decide quando defender, recuperar energia ou usar fogo. Vitória inicia evento de Brian e epílogo. O sacrifício é narrativo, sem encontro adicional.'),
      ('XP proposta, a testar', 'Nível 1: 0 XP; nível 2: 40 XP acumulados; nível 3: 120 XP. Recompensas E1 a E6: 20, 20, 35, 45, 40 e 60 XP (220 ao final). Assim Magnus inicia mapa 2 no nível 2 e mapa 3 no nível 3; nenhum grind é necessário. Estes valores são novos parâmetros de planejamento, sem alegação de balanceamento testado.'),
      ('Escalada e limites', 'Aumentar primeiro a complexidade das decisões, depois a resistência dos inimigos. Números de dano, vida e energia serão definidos na v0.3. Medir duração, derrotas e compreensão dos avisos; se houver bloqueio, ajustar custos e dano antes de exigir XP adicional. Duração total estimada: 20 a 30 minutos.'),
     ]),
     ('Validação e entrega v0.2', [
      ('Material incluído', 'GDD atualizado editável e em PDF; dez PNGs preservados e catalogados; model sheet preliminar em quatro vistas; concepts elementais e arena; identidade visual; planejamento de sprites, cenários, mapas e progressão. As páginas de plantas especificam rotas e bloqueios. A v0.1 permanece arquivada.'),
      ('Limites e próximos passos', 'Concluir concepts de NPCs e inimigos, finalizar turnaround neutro, produzir cenários dos mapas 1 e 2 e converter referências em sprites. Esses refinamentos ficam explicitamente pendentes; o material desta entrega é direção visual e planejamento, não assets finais nem protótipo.'),
      ('Validação v0.3 e final', 'V0.3: movimento com colisão, pista, batalha completa, XP único e reinício após derrota; verificar toque em Android. Final: concluir três mapas sem bloqueio, entender objetivos, salvar/retomar sem duplicar recompensas e concluir epílogo. Registrar plataforma, build, duração e resultado de cada teste. Ainda não há resultados de gameplay.'),
      ('Repositório', 'Remoto configurado: https://github.com/Matheus-Hen/entre-fogo-e-agua-2d. A tag v0.2 deve identificar o commit revisado desta entrega. A presença no remoto deve ser verificada após o envio; este texto não comprova publicação. README centraliza links e a matriz dos requisitos.'),
      ('Créditos e decisões pendentes', 'Confirmar autoria do manuscrito e procedência/licenças das imagens; validar papéis da equipe e parâmetros técnicos. As imagens foram fornecidas pelo usuário, sem metadados de autoria confirmados. Novas definições visuais e de level design neste documento são propostas coerentes com o GDD base.'),
      ('Histórico', 'v0.1 - 10/09/2026: concepção inicial relativa à etapa de 03/09. v0.2 - 10/09/2026: organização dos materiais, direção visual, model sheet de referência, planejamento de animação, cenários, rotas das três fases e progressão dos seis encontros.'),
     ]),
    ]
    return pages

def main():
    pages=sections()
    styles={
      'title':ParagraphStyle('title',fontName='Helvetica-Bold',fontSize=23,leading=27,textColor=HexColor('#152C3D'),spaceAfter=15),
      'heading':ParagraphStyle('heading',fontName='Helvetica-Bold',fontSize=11,leading=14,textColor=HexColor('#A64B24'),spaceBefore=10,spaceAfter=4,keepWithNext=True),
      'body':ParagraphStyle('body',fontName='Helvetica',fontSize=10,leading=14,textColor=HexColor('#243746'),spaceAfter=5),
      'caption':ParagraphStyle('caption',fontName='Helvetica',fontSize=9,leading=12,textColor=HexColor('#526878'),spaceAfter=10),
    }
    flow=[]; md=['# Between fire and water','','## GDD v0.2 | Direção Visual e Level Design | 10/09/2026','']
    def page(title,items,images=(),maxheight=280):
        if flow: flow.append(PageBreak())
        flow.append(Paragraph(escape(title),styles['title']));md.extend(['## '+title,''])
        for path,label in images:
            full=ROOT/'imagens'/path
            with PILImage.open(full) as im: w,h=im.size
            scale=min(510/w,maxheight/h)
            flow.extend([Image(str(full),width=w*scale,height=h*scale),Spacer(1,6),Paragraph(escape(label),styles['caption'])])
            md.extend([f'![{label}](../imagens/{path})',''])
        for heading,body in items:
            flow.extend([Paragraph(escape(heading),styles['heading']),Paragraph(escape(body),styles['body'])]);md.extend(['### '+heading,'',body,''])
    for title,items in pages:
        page(title,items)
        if title=='Magnus - model sheet de referência':
            page('Model sheet | frente e costas', [('Uso','Consultar roupa, silhueta e proporções. As poses são de locomoção e exigem alinhamento antes do uso como sprites.')],[(p,l) for _,p,l in ASSETS[:2]],180)
            page('Model sheet | perfis', [('Uso','Conferir continuidade entre direita e esquerda. Não presumir simetria perfeita nem compatibilidade automática entre frames.')],[(p,l) for _,p,l in ASSETS[2:4]],180)
        if title=='Direção visual e referências':
            page('Concept art | fogo e água',[('Aplicação','Fogo enfatiza impacto; água reforça fluxo e proteção. Fundo opaco e espaçamento devem ser corrigidos na produção. Cada prancha é referência de estágios do efeito.')],[(p,l) for _,p,l in ASSETS[4:6]],210)
        if title=='Cenários - linguagem por fase':
            page('Concept de cenário | arena ASDA',[('Direção','Reservar espaço livre para Magnus, inimigo e menu de ações. A iluminação do reator reforça o foco central; o fundo não adiciona mecânicas. Produzir uma versão de exploração com a câmera superior prevista no GDD.')],[(ASSETS[6][1],ASSETS[6][2])],320)
        if title=='Level design - regras de leitura':
            page('Mapa 1 | acampamento e templo',[
             ('Rota principal','Entrada sul > pátio central (E1/Ki) > templo norte (losango: pista da traição) > retorno ao pátio > passagem leste (E2/guarda) > saída leste.'),
             ('Bloqueios e área opcional','Acesso à pista após E1; fuga liberada pela pista e vitória em E2. Área oeste funciona como espaço opcional de ambientação, sem combate ou XP. Saída ao norte do templo não é transição jogável nesta adaptação.'),
            ],[(ASSETS[7][1],ASSETS[7][2])],320)
            page('Mapa 2 | posto DASA',[
             ('Rota principal','Entrada na recepção sul > corredor central (E3) > arquivo oeste (losango: identificação) e sala leste (E4; losango: registro da base) > porta norte para ASDA.'),
             ('Bloqueios e liberdade','Após E3, visitar oeste e leste em qualquer ordem. E4 protege a pista leste. Porta norte verifica E3, E4 e as duas pistas. Recepção serve de orientação e diálogo com aliados; não contém encontro extra.'),
            ],[(ASSETS[8][1],ASSETS[8][2])],320)
            page('Mapa 3 | base ASDA',[
             ('Rota principal','Entrada sul > corredor (círculo: E5/elite) > antecâmara > arena norte (círculo: E6/Mark) > evento de Brian e epílogo.'),
             ('Bloqueios e conclusão','E5 libera a antecâmara; interagir com entrada da arena inicia diálogo e E6. Vitória encerra exploração e inicia o final. Não adicionar outra fase nem combate ao reator. Retentativa de E6 restaura estado pré-encontro sem repetir recompensa de E5.'),
            ],[(ASSETS[9][1],ASSETS[9][2])],320)
    (ROOT/'docs/GDD-v0.2.md').write_text('\n'.join(md),encoding='utf-8')
    def frame(c,doc):
        w,h=doc.pagesize;c.setFillColor(HexColor('#152C3D'));c.rect(0,h-46,w,46,fill=1,stroke=0)
        c.setFillColor(HexColor('#FFFFFF'));c.setFont('Helvetica-Bold',10);c.drawString(42,h-28,'BETWEEN FIRE AND WATER')
        for x,color in [(42,'#E17A3C'),(112,'#259EBC')]:c.setFillColor(HexColor(color));c.rect(x,h-55,70,3,fill=1,stroke=0)
        c.setFillColor(HexColor('#526878'));c.setFont('Helvetica',8);c.drawString(42,25,'GDD v0.2 | Direção Visual e Level Design | 10/09/2026');c.drawRightString(w-42,25,str(doc.page))
    target=ROOT/'output/pdf/Between-fire-and-water-GDD-v0.2.pdf'
    SimpleDocTemplate(str(target),pagesize=(595.28,841.89),rightMargin=42,leftMargin=42,topMargin=75,bottomMargin=47,title='Between fire and water - GDD v0.2',author='Equipe Between fire and water').build(flow,onFirstPage=frame,onLaterPages=frame)
    print(target)

if __name__=='__main__':main()
