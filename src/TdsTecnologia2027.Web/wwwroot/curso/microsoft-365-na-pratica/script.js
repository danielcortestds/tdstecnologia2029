// ===== Trilha do curso (módulos e aulas do Microsoft 365 na Prática) =====
const modules = [
  ['Boas-vindas e primeiros passos', 4, [
    'Apresentação do curso',
    'Descubra qual é o seu plano',
    'O portal e os aplicativos',
    'Qual ferramenta usar para cada tarefa'
  ]],
  ['Outlook: e-mail e agenda', 3, [
    'Outlook: caixa de entrada organizada',
    'Outlook: regras automáticas',
    'Outlook: marcar uma reunião na agenda'
  ]],
  ['Teams: chat e equipes', 1, [
    'Teams: chat e equipes, o essencial'
  ]],
  ['OneDrive e SharePoint', 3, [
    'Onde salvar: OneDrive ou Teams',
    'Compartilhar e editar junto',
    'Versões e lixeira'
  ]],
  ['Planner e To Do', 2, [
    'Planner: tarefas organizadas',
    'To Do: a sua lista do dia'
  ]],
  ['Forms: pesquisa rápida', 1, [
    'Forms: pesquisa rápida'
  ]],
  ['Copilot Chat', 1, [
    'Copilot Chat no dia a dia'
  ]],
  ['Apps desktop e Clipchamp (planos Standard e superiores)', 2, [
    'Aplicativos desktop: instalar e quando usar',
    'Clipchamp: um tutorial em vídeo'
  ]],
  ['Desafio final e encerramento', 1, [
    'Desafio final e certificado'
  ]]
];
document.querySelector('#modules').innerHTML = modules.map((m, i) =>
  `<details><summary><span class="num">0${i + 1}</span><span>${m[0]}<small>${m[1]} ${m[1] > 1 ? 'AULAS' : 'AULA'}</small></span></summary><ul>${m[2].map(a => `<li>${a}</li>`).join('')}</ul></details>`
).join('');

// ===== Demonstração: como a maioria faz x como você vai fazer =====
const tasks = [
  {
    badAsk: 'Abre a caixa de entrada e lê tudo, um e-mail por vez, procurando o que é importante.',
    badAnswer: 'A mensagem do cliente se perde entre newsletters e cópias. O prazo de sexta passa despercebido.',
    goodAsk: '<b>Destaques e Outros</b> ligado, <b>categoria</b> do projeto e uma <b>regra automática</b> que leva os e-mails para a pasta certa.',
    goodAnswer: 'A caixa já chega separada: o que importa em Destaques, o projeto na sua pasta e o e-mail com prazo sinalizado aparece também no To Do.'
  },
  {
    badAsk: 'Troca oito e-mails para achar um horário e depois manda a pauta por mensagem.',
    badAnswer: 'Metade da equipe chega sem ler a pauta e o link da chamada se perde no meio da conversa.',
    goodAsk: '<b>Novo evento</b> no Calendário do Outlook, com <b>Reunião do Teams</b> ativada e a <b>pauta</b> no próprio convite.',
    goodAnswer: 'Horário, link do Teams e pauta no mesmo lugar. Quem aceita o convite já tem tudo na agenda.'
  },
  {
    badAsk: 'Envia a planilha por e-mail como anexo: "segue a versão final_v3_revisada".',
    badAnswer: 'Cada pessoa edita uma cópia diferente e ninguém sabe qual é a versão certa.',
    goodAsk: 'Arquivo salvo no <b>Teams da equipe</b> e compartilhado com o link <b>Pessoas que você escolher</b>.',
    goodAnswer: 'Todos editam o mesmo arquivo, ao mesmo tempo. Se algo der errado, o Histórico de versões traz a anterior de volta.'
  },
  {
    badAsk: 'Tarefas anotadas no papel, no bloco de notas e em mensagens soltas no chat.',
    badAnswer: 'Ninguém sabe quem faz o quê. A cobrança acontece no corredor, quando o prazo já passou.',
    goodAsk: 'Plano no <b>Planner</b> com as colunas A fazer, Em andamento e Concluído, com <b>responsável e data</b> em cada tarefa.',
    goodAnswer: 'O quadro mostra o andamento da equipe, e cada pessoa vê as próprias tarefas no Meu Dia do To Do.'
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


// ===== Calculadora para o RH: horas devolvidas ao trabalho =====
(() => {
  const people = document.querySelector('#roi-people');
  const minutes = document.querySelector('#roi-minutes');
  const out = document.querySelector('#roi-hours');
  const text = document.querySelector('#roi-text');
  if (!people || !minutes || !out) return;
  const calc = () => {
    const p = Math.max(0, Math.min(100000, parseInt(people.value, 10) || 0));
    const m = Math.max(0, Math.min(240, parseInt(minutes.value, 10) || 0));
    const hours = Math.round(p * m * 21 / 60);
    out.textContent = hours.toLocaleString('pt-BR');
    text.textContent = `horas por mês de volta para o trabalho (${p.toLocaleString('pt-BR')} pessoas × ${m} min por dia × 21 dias úteis)`;
  };
  people.addEventListener('input', calc);
  minutes.addEventListener('input', calc);
  calc();
})();

// ===== Animação de entrada (respeita prefers-reduced-motion) =====
(() => {
  const preference = window.matchMedia('(prefers-reduced-motion: reduce)');
  if (preference.matches || !('IntersectionObserver' in window)) return;
  const targets = document.querySelectorAll('.project-head,.chat-demo,.project-results,.audience-intro,.cards>div,.company-head,.hooks>div,.roi,.curriculum-intro,.modules>details,.offer>div,.faq>div');
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
