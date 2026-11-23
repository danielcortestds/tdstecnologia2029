// ===== Trilha do curso (módulos e aulas do EXIN BCS AI Foundation) =====
const modules = [
  ['Boas-vindas e contexto', [
    'Boas-vindas: o curso, o instrutor e a prova',
    'EXIN e BCS: uma certificação de padrão mundial'
  ]],
  ['Introdução à IA e evolução histórica', [
    'Definições-chave e tipos de IA',
    'Marcos da história da IA',
    'O impacto da IA na sociedade e a IA sustentável'
  ]],
  ['Considerações éticas e legais', [
    'Ética, preocupações e governança de IA',
    'Desafios éticos em projetos de IA e como enfrentá-los',
    'Regulação e gestão de riscos em IA'
  ]],
  ['Facilitadores da IA', [
    'Exemplos de IA e robótica',
    'Machine learning, redes neurais, deep learning e LLMs',
    'Tarefas e tipos de aprendizado de máquina'
  ]],
  ['Encontrando e usando dados na IA', [
    'Tipos de dados e qualidade de dados',
    'Riscos, big data e visualização de dados',
    'IA generativa e LLMs',
    'Como os dados treinam a IA: o processo de machine learning'
  ]],
  ['Usando IA na sua organização', [
    'Oportunidades, business case e stakeholders',
    'Projetos, riscos, custos e benefícios',
    'Governança contínua da IA'
  ]],
  ['Futuro e impacto: humano + máquina', [
    'Carreiras e IA no mundo real',
    'O impacto da IA na sociedade e o futuro',
    'Consciência e IA ética'
  ]],
  ['Preparação para a certificação', [
    'Como é a prova e como responder',
    'Simulado comentado: 15 questões'
  ]]
];
document.querySelector('#modules').innerHTML = modules.map((m, i) =>
  `<details><summary><span class="num">0${i + 1}</span><span>${m[0]}<small>${m[1].length} ${m[1].length === 1 ? 'AULA' : 'AULAS'}</small></span></summary><ul>${m[1].map(a => `<li>${a}</li>`).join('')}</ul></details>`
).join('');

// ===== Demonstração: avaliação de um caso de uso de IA =====
const concerns = ['Viés e discriminação', 'Privacidade', 'Transparência', 'Responsabilização', 'Segurança', 'Qualidade dos dados'];
const cases = [
  { area: 'RECURSOS HUMANOS', title: 'IA que pré-seleciona candidatos', p: 4, i: 5, response: 'Mitigar',
    on: [0, 1, 2, 3],
    actions: ['Testar viés nos dados de treino e nos resultados', 'Revisão humana em toda decisão de seleção', 'Informar os candidatos sobre o uso de IA', 'Registrar decisões e auditar periodicamente'] },
  { area: 'SETOR PÚBLICO', title: 'Assistente virtual de atendimento ao cidadão', p: 3, i: 4, response: 'Mitigar',
    on: [1, 2, 4],
    actions: ['Avisar que o atendimento é feito por um assistente virtual', 'Encaminhar para um atendente humano quando necessário', 'Coletar só os dados pessoais necessários', 'Monitorar respostas e reclamações'] },
  { area: 'COMPLIANCE E JURÍDICO', title: 'IA que resume e analisa contratos', p: 3, i: 3, response: 'Mitigar',
    on: [1, 3, 4],
    actions: ['Usar só ferramentas corporativas aprovadas', 'Conferir as respostas com o contrato original', 'Decisão final sempre de um profissional', 'Definir um responsável pelo uso da ferramenta'] },
  { area: 'OPERAÇÕES', title: 'Previsão de demanda de estoque', p: 2, i: 2, response: 'Aceitar e monitorar',
    on: [2, 5],
    actions: ['Checar a qualidade e a atualização dos dados', 'Comparar a previsão com o realizado todo mês', 'Manter a decisão final com o gestor'] }
];
const level = s => s >= 15 ? ['alto', 'RISCO ALTO'] : s >= 8 ? ['medio', 'RISCO MÉDIO'] : ['baixo', 'RISCO BAIXO'];
const cellColor = s => s >= 15 ? '#f97066' : s >= 8 ? '#fdb022' : '#6ce9a6';
function renderCase(n) {
  const c = cases[n];
  const [cls, label] = level(c.p * c.i);
  document.querySelector('#case-area').textContent = c.area;
  document.querySelector('#case-title').textContent = c.title;
  const badge = document.querySelector('#case-badge');
  badge.className = 'risk-badge ' + cls;
  badge.textContent = label;
  document.querySelector('#case-response').textContent = c.response;
  let cells = '';
  for (let p = 5; p >= 1; p--) for (let i = 1; i <= 5; i++) {
    const on = p === c.p && i === c.i;
    cells += `<span class="${on ? 'on' : ''}" style="background:${cellColor(p * i)};opacity:${on ? 1 : .45}"></span>`;
  }
  document.querySelector('#matrix').innerHTML = cells;
  document.querySelector('#chips').innerHTML = concerns.map((t, k) => `<span class="${c.on.includes(k) ? 'on' : ''}">${t}</span>`).join('');
  document.querySelector('#case-actions').innerHTML = c.actions.map(a => `<li>${a}</li>`).join('');
  document.querySelectorAll('[data-case]').forEach(b => {
    b.classList.toggle('selected', +b.dataset.case === n);
    b.setAttribute('aria-pressed', +b.dataset.case === n);
  });
}
document.querySelectorAll('[data-case]').forEach(b => b.addEventListener('click', () => renderCase(+b.dataset.case)));
renderCase(0);

// ===== Animação de entrada (respeita prefers-reduced-motion) =====
(() => {
  const preference = window.matchMedia('(prefers-reduced-motion: reduce)');
  if (preference.matches || !('IntersectionObserver' in window)) return;
  const targets = document.querySelectorAll('.project-head,.dashboard,.project-results,.audience-intro,.cards>div,.company-head,.hooks>div,.exam-map,.curriculum-intro,.modules>details,.offer>div,.faq>div');
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
