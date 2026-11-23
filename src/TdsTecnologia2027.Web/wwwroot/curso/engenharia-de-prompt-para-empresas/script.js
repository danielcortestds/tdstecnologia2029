// ===== Trilha do curso (módulos e aulas do Engenharia de Prompt para Empresas) =====
const modules = [
  ['Fundamentos: como a IA pensa (e erra)', 3, [
    'Boas-vindas: o curso e como aproveitar',
    'Como a IA generativa funciona, sem matemática',
    'Onde a IA erra: alucinação, data de corte e limites'
  ]],
  ['O método CRIAR', 3, [
    'Prompt fraco x prompt bom',
    'O método CRIAR, letra por letra',
    'Contexto é tudo: anexos, dados e o que a IA não sabe'
  ]],
  ['Técnicas que mais dão resultado', 3, [
    'Exemplos e modelos de saída',
    'Dividir para conquistar: passo a passo e tarefas em etapas',
    'Conversar para refinar: iteração, perguntas e autorrevisão'
  ]],
  ['ChatGPT, Claude, Gemini e Copilot na prática', 3, [
    'Quatro ferramentas, um mesmo método',
    'Recursos que multiplicam: arquivos, pesquisa, projetos e assistentes',
    'Versão gratuita ou corporativa? O que muda para a empresa'
  ]],
  ['Casos de uso no dia a dia', 4, [
    'E-mails e comunicação',
    'Reuniões, documentos longos e resumos',
    'Planilhas, dados e relatórios',
    'Prompts por área: RH, comercial, financeiro, jurídico e atendimento'
  ]],
  ['Uso seguro e responsável', 3, [
    'O que nunca colar na IA',
    'A IA não assina por você: checagem, viés e responsabilidade',
    'A política de uso de IA da empresa'
  ]],
  ['Projeto final e próximos passos', 2, [
    'Projeto final: um dia de trabalho com IA, passo a passo',
    'Encerramento: os hábitos que ficam'
  ]],
  ['Bônus: conectando a IA às suas ferramentas (MCP)', 3, [
    'O que é MCP: a tomada universal da IA',
    'Passo a passo: conectando o Google Drive',
    'Conectores com segurança'
  ]]
];
document.querySelector('#modules').innerHTML = modules.map((m, i) =>
  `<details><summary><span class="num">0${i + 1}</span><span>${m[0]}<small>${m[1]} AULAS</small></span></summary><ul>${m[2].map(a => `<li>${a}</li>`).join('')}</ul></details>`
).join('');

// ===== Demonstração: pedido vago x pedido com o método CRIAR =====
const tasks = [
  {
    badAsk: 'responde essa reclamação',
    badAnswer: 'Prezada cliente,\nLamentamos o ocorrido. Estamos verificando a situação e retornaremos em breve.\nAtenciosamente.',
    goodAsk: '<b>C</b> Sou do atendimento de uma distribuidora de alimentos. A cliente Júlia recebeu 3 caixas avariadas. <b>R</b> Tom cordial, até 8 linhas, sem prometer o que não posso cumprir. <b>I</b> Escreva a resposta. <b>A</b> Siga o padrão: desculpas, solução, prazo. <b>R</b> E-mail com assunto.',
    goodAnswer: 'Assunto: Reposição das caixas avariadas\n\nOlá, Júlia!\nPeço desculpas pelas caixas que chegaram avariadas. Já separamos a reposição, que sai amanhã no primeiro horário, e vamos revisar o empilhamento das cargas.\nComo forma de desculpas, o seu próximo pedido terá 10% de desconto.\nConte com a gente!'
  },
  {
    badAsk: 'resume essa reunião',
    badAnswer: 'A reunião tratou de vendas, clientes e logística. Foram discutidos vários pontos e a equipe ficou de dar andamento.',
    goodAsk: '<b>C</b> Transcrição da reunião comercial de hoje. <b>R</b> Use só o que está no texto; se faltar prazo, escreva "não definido". <b>I</b> Faça a ata. <b>A</b> Decisões em tópicos e tarefas em tabela. <b>R</b> Tabela: o quê | responsável | prazo.',
    goodAnswer: 'Decisões\n• Proposta para o novo cliente aprovada\n\nTarefas\n• Enviar proposta · Carla · sexta\n• Revisar preços da linha de grãos · Henrique · não definido\n• Ligar para o restaurante sobre a avaria · Fábio · amanhã'
  },
  {
    badAsk: 'analisa essas vendas',
    badAnswer: 'A planilha mostra as vendas por região e por produto. Os valores variaram ao longo do trimestre.',
    goodAsk: '<b>C</b> Planilha de vendas do trimestre por região e produto. <b>R</b> Não invente números; mostre o cálculo. <b>I</b> Encontre as maiores altas e quedas. <b>A</b> Formato "Região/Produto: variação %". <b>R</b> 3 tópicos para a diretoria.',
    goodAnswer: '• Total do trimestre: R$ 2.062.300 → R$ 2.113.200 (+2,5%)\n• Maior alta: Capital/Café, +34,0%\n• Maior queda: Sul/Farinha, −29,0%: vale investigar com o time do Sul.'
  },
  {
    badAsk: 'o que diz esse contrato?',
    badAnswer: 'O contrato trata da prestação de serviços de transporte, com cláusulas sobre prazos, valores e responsabilidades das partes.',
    goodAsk: '<b>C</b> Contrato de transporte com uma transportadora parceira. <b>R</b> Cite sempre o número da cláusula. <b>I</b> Explique o que vale em caso de avaria. <b>A</b> Linguagem simples, sem juridiquês. <b>R</b> Até 5 tópicos.',
    goodAnswer: '• A transportadora responde pelas avarias no transporte (Cláusula 5)\n• Ela ressarce o valor de custo do que foi danificado\n• Prazo: até 15 dias após a comunicação, com fotos (Cláusula 5)\n• Empilhar a carga conforme as instruções é obrigação dela (Cláusula 4)\n\nConfira as cláusulas citadas antes de decidir.'
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
