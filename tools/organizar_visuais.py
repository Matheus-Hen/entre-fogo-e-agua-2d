"""Copia o acervo fornecido, preserva os bytes e gera inventário verificável."""
from pathlib import Path
from collections import Counter
import hashlib
import json
import shutil
from PIL import Image

ROOT = Path(__file__).resolve().parents[1]
SOURCE = Path(r'C:\Users\Teste\Documents')
FILES = [
 ('acampamento','Art/Concepts/Environments/Acampamento/acampamento-exploracao.png'),
 ('asda','Art/Concepts/Environments/PostoDASA/posto-dasa-exploracao.png'),
 ('Brian','Art/Concepts/Characters/Brian/brian-retrato.png'),
 ('fundos-conceituais','Art/Concepts/Boards/cenarios-e-objetos-prancha.png'),
 ('guardas','Art/Concepts/Characters/Guardas/guardas-comum-e-elite.png'),
 ('Ki','Art/Concepts/Characters/Ki/ki-concept.png'),
 ('Lizzie','Art/Concepts/Characters/Lizzie/lizzie-concept.png'),
 ('magnus','Art/Concepts/Characters/Magnus/magnus-concept.png'),
 ('magnus-detalhes-complementares','Art/ModelSheets/Magnus/magnus-detalhes.png'),
 ('magnus-expressoes','Art/ModelSheets/Magnus/magnus-expressoes.png'),
 ('Mark','Art/Concepts/Characters/Mark/mark-concept.png'),
 ('Michael','Art/Concepts/Characters/Michael/michael-retrato.png'),
 ('modelsheet-construcao','Art/ModelSheets/Magnus/magnus-vistas-construcao.png'),
 ('objetos investigaveis','Art/Concepts/Props/objetos-investigaveis-prancha.png'),
 ('Olive','Art/Concepts/Characters/Olive/olive-retrato.png'),
 ('acampamento-lateral.png','Art/BattleBackgrounds/Acampamento/acampamento-batalha.png'),
 ('base-asda.png','Art/Concepts/Environments/BaseASDA/base-asda-exploracao.png'),
 ('base-asda-lateral.png','Art/BattleBackgrounds/BaseASDA/base-asda-batalha.png'),
 ('fase_1_mapa_progressao.png','LevelDesign/Fase01-Acampamento/fase01-progressao.png'),
 ('fire-element.png','Art/AnimationStudies/Effects/fogo-sequencia.png'),
 ('magnus-caminhando-back.png','Art/AnimationStudies/Magnus/magnus-caminhada-costas.png'),
 ('magnus-caminhando-front.png','Art/AnimationStudies/Magnus/magnus-caminhada-frente.png'),
 ('magnus-caminhando-left.png','Art/AnimationStudies/Magnus/magnus-caminhada-esquerda.png'),
 ('magnus-caminhando-rigth.png','Art/AnimationStudies/Magnus/magnus-caminhada-direita.png'),
 ('posto-asda-lateral.png','Art/BattleBackgrounds/PostoDASA/posto-dasa-batalha.png'),
 ('scketch-fase1.png','LevelDesign/Fase01-Acampamento/fase01-sketch.png'),
 ('scketch-fase2.png','LevelDesign/Fase02-PostoDASA/fase02-sketch.png'),
 ('scketch-fase3.png','LevelDesign/Fase03-BaseASDA/fase03-sketch.png'),
 ('water-element.png','Art/AnimationStudies/Effects/agua-sequencia.png'),
]

def sha(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()

def main():
    records=[]
    # Confere todas as entradas e conflitos antes de copiar.
    for name,relative in FILES:
        source=SOURCE/name
        destination=ROOT/relative
        digest=sha(source)
        if destination.exists() and sha(destination)!=digest:
            raise RuntimeError(f'Destino existente com conteúdo diferente: {destination}')
        with Image.open(source) as img:
            img.verify()
        with Image.open(source) as img:
            if img.format!='PNG':
                raise ValueError(f'Formato inesperado: {source}')
            alpha=img.getchannel('A').getextrema() if 'A' in img.getbands() else None
            records.append(dict(origem=str(source),arquivo=relative,sha256=digest,
                formato=img.format,largura=img.width,altura=img.height,bytes=source.stat().st_size,
                transparencia=bool((alpha and alpha[0]<255) or 'transparency' in img.info),
                status='Referencia visual recebida; integracao Unity pendente'))
    for record in records:
        dest=ROOT/record['arquivo']
        dest.parent.mkdir(parents=True,exist_ok=True)
        if not dest.exists(): shutil.copy2(record['origem'],dest)
        assert sha(dest)==record['sha256']
    (ROOT/'Art/inventario.json').write_text(json.dumps(records,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
    lines=['# Acervo visual do jogo','',
      f'{len(records)} imagens fornecidas pelo usuário, organizadas sem alteração de pixels. Os arquivos originais em Documentos foram preservados. Todas as cópias foram verificadas por SHA-256.','',
      '## Estrutura','',
      '- `Concepts/Characters`: personagens e retratos.',
      '- `Concepts/Environments`: conceitos de exploração por cenário.',
      '- `Concepts/Props` e `Concepts/Boards`: objetos e pranchas conjuntas.',
      '- `ModelSheets/Magnus`: vistas, detalhes e expressões.',
      '- `BattleBackgrounds`: fundos laterais por cenário.',
      '- `AnimationStudies`: sequências de caminhada e efeitos elementais.',
      '- `../LevelDesign`: sketches e progressão das fases.','',
      '## Estado de produção','',
      'Este acervo contém conceitos, pranchas e estudos recebidos. Caminhadas e efeitos ainda precisam de recorte, alinhamento e preparação de transparência; não são animações integradas ou sprites finais. As imagens de cenário não constituem fases jogáveis. Autoria, ferramenta e licença de origem ainda devem ser informadas pela equipe.','',
      'Os arquivos originais `asda` e `posto-asda-lateral.png` foram agrupados como Posto DASA pelo conteúdo de recepção/empresa de segurança e pela nomenclatura do GDD; a base final permanece como Base ASDA. O inventário mantém os nomes originais para rastreabilidade.','',
      'O guia PDF v0.2 documenta o planejamento anterior ao recebimento destas imagens. A existência atual das artes está registrada neste índice.','',
      '## Índice de arquivos','',
      '| Arquivo organizado | Original | Dimensões |',
      '| --- | --- | --- |']
    for r in records:
        relative='../'+r['arquivo']
        lines.append(f"| [{r['arquivo']}]({relative}) | {Path(r['origem']).name} | {r['largura']} x {r['altura']} |")
    lines += ['', '[Inventário técnico com hashes e origem](inventario.json)', '']
    # O índice do acervo fica centralizado no README da raiz e no GDD.
    print(f'Arquivos verificados: {len(records)}; total: {sum(r["bytes"] for r in records)/1024/1024:.1f} MiB')
    print('Hashes repetidos:',sum(n-1 for n in Counter(r['sha256'] for r in records).values()))

if __name__=='__main__': main()
