// ===== Trilha do curso (módulos e aulas do ChatGPT e Claude no Trabalho) =====
const modules = [
  ['Comece aqui', 2, [
    'IA em 3 minutos',
    'ChatGPT e Claude lado a lado'
  ]],
  ['Como pedir bem', 2, [
    'As 4 perguntas e o ajuste',
    'Desconfie e confira'
  ]],
  ['Escrita do dia a dia', 3, [
    'E-mails e mensagens',
    'Revisar textos',
    'Atas e resumos: da gravação da reunião à ata pronta'
  ]],
  ['Arquivos e dados', 4, [
    'Conversar com PDF e Word',
    'Planilhas sem medo',
    'Fotos e prints no celular',
    'Pedir arquivos prontos'
  ]],
  ['Pesquisa e organização', 3, [
    'Pesquisar com fontes',
    'Ideias, plano e reunião',
    'Projetos e instruções pessoais'
  ]],
  ['Uso seguro e seu kit', 2, [
    'O que nunca colar na IA',
    'Seu kit para amanhã'
  ]],
  ['Bônus: fotos e vídeos com IA', 3, [
    'Tratar fotos com o ChatGPT',
    'Vídeo curto com o Claude',
    'Sua Skill de imagens no Claude'
  ]]
];
document.querySelector('#modules').innerHTML = modules.map((m, i) =>
  `<details><summary><span class="num">0${i + 1}</span><span>${m[0]}<small>${m[1]} AULAS</small></span></summary><ul>${m[2].map(a => `<li>${a}</li>`).join('')}</ul></details>`
).join('');

// ===== Demonstração: pedido vago x pedido com as 4 perguntas =====
const tasks = [
  {
    badAsk: 'responde esse cliente',
    badAnswer: 'Prezado cliente,\nAgradecemos o seu contato. Estamos verificando a situação e retornaremos em breve.\nAtenciosamente.',
    goodAsk: '<b>Sou</b> atendente de uma loja de móveis. <b>Quero</b> responder a Marina, que reclamou do atraso da entrega. <b>Objetivo:</b> acalmar e manter a cliente. <b>Formato:</b> tom cordial, até 6 linhas, com a nova data (28/11).',
    goodAnswer: 'Olá, Marina!\nPeço desculpas pelo atraso no seu pedido. Entendo a sua frustração. Já priorizamos o envio, e a nova data de entrega é sexta-feira, 28/11.\nVou acompanhar pessoalmente e te aviso assim que sair para entrega.\nObrigada pela paciência!'
  },
  {
    badAsk: 'faz um resumo dessa reunião',
    badAnswer: 'A reunião tratou de vendas, campanha e avarias. Foram discutidos vários pontos importantes e a equipe ficou de dar andamento.',
    goodAsk: '<b>Sou</b> coordenador comercial. <b>Quero</b> uma ata desta transcrição. <b>Para</b> enviar à equipe hoje. <b>Formato:</b> decisões e uma tabela de próximos passos com responsável e prazo. Se faltar algo, marque "a confirmar".',
    goodAnswer: 'Decisões: proposta da rede Grão Fino aprovada.\n\nPróximos passos\n• Enviar proposta à Grão Fino · Bruno · 14/11\n• Revisar campanha de fim de ano · Letícia · 18/11\n• Levantar avarias de outubro · Henrique · a confirmar'
  },
  {
    badAsk: 'analisa essa planilha',
    badAnswer: 'A planilha contém dados de vendas por mês e por vendedor. Os valores variam ao longo do período.',
    goodAsk: '<b>Sou</b> analista de vendas. <b>Quero</b> entender esta planilha de 2025. <b>Para</b> apresentar ao gerente amanhã. <b>Formato:</b> 3 destaques em tópicos e a fórmula de Excel para somar as vendas por vendedor.',
    goodAnswer: '• Março foi o melhor mês: R$ 84 mil (+22%).\n• Ana responde por 31% das vendas do ano.\n• Junho caiu 15%: vale investigar.\n\nFórmula: =SOMASE(B:B;"Ana";D:D)'
  },
  {
    badAsk: 'o que diz esse contrato?',
    badAnswer: 'O contrato trata da prestação de serviços entre as partes, com cláusulas sobre prazos, pagamentos e responsabilidades.',
    goodAsk: '<b>Sou</b> do administrativo. <b>Quero</b> entender este contrato de 18 páginas. <b>Para</b> decidir se renovamos. <b>Formato:</b> prazo, valor, multa por cancelamento e reajuste, citando a cláusula de cada um.',
    goodAnswer: '• Prazo: 12 meses, renovação automática (cl. 3.1)\n• Valor: R$ 4.800/mês (cl. 5.2)\n• Multa por cancelamento: 2 mensalidades (cl. 9.4)\n• Reajuste: IPCA anual (cl. 5.5)\n\nConfira as cláusulas citadas antes de decidir.'
  }
];
function renderTask(n) {
  const t = tasks[n];
  document.querySelector('#bad-ask').textContent = t.badAsk;
  document.querySelector('#bad-answer').textContent = t.badAnswer;
  document.querySelector('#good-ask').innerHTML = t.goodAsk;
  document.querySelector('#good-answer').textContent = t.goodAnswer;
  document.querySelectorAll('[data-task]').forEach(b => {
    b.classList.toggle('selected', +b.dataset.task === n);
    b.setAttribute('aria-pressed', +b.dataset.task === n);
  });
}
document.querySelectorAll('[data-task]').forEach(b => b.addEventListener('click', () => renderTask(+b.dataset.task)));
renderTask(0);

// ===== Animação de entrada (respeita prefers-reduced-motion) =====
(() => {
  const preference = window.matchMedia('(prefers-reduced-motion: reduce)');
  if (preference.matches || !('IntersectionObserver' in window)) return;
  const targets = document.querySelectorAll('.project-head,.chat-demo,.project-results,.audience-intro,.cards>div,.curriculum-intro,.modules>details,.offer>div,.faq>div');
  const observer = new IntersectionObserver(entries => {
    entries.forEach(entry => {
      if (entry.isIntersecting) { entry.target.classList.remove('tds-pending'); entry.target.classList.add('tds-visible'); observer.unobserve(entry.target); }
    });
  }, { threshold: 0, rootMargin: '0px 0px -35px 0px' });
  targets.forEach(el => {
    if (el.getBoundingClientRect().top < window.innerHeight) return;
    el.classList.add('tds-reveal', 'tds-pending');
    observer.observe(el);
  });
})();

// ===== Destino da compra (centralizado no config.js) =====
(() => {
  const config = window.TDS_CONFIG;
  if (!config) return;
  let destination;
  try { destination = new URL(config.checkoutUrl, window.location.href); } catch { return; }
  if (!['https:', 'http:'].includes(destination.protocol)) return;
  document.querySelectorAll('.tds-purchase').forEach(link => { link.href = destination.href; });
})();
