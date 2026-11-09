// ARQUIVO GERADO automaticamente (carga dos cursos online Mashup e LGPD).
using Microsoft.EntityFrameworkCore;
using TdsTecnologia2027.Web.Models;

namespace TdsTecnologia2027.Web.Data;

public static class SeedCursosOnline
{
    /// <summary>Cria os cursos online Mashup e LGPD (com módulos e aulas) se ainda não existirem.</summary>
    public static async Task ExecutarAsync(ApplicationDbContext db)
    {
        await GarantirAsync(db, Mashup());
        await GarantirAsync(db, Lgpd());
        await GarantirAsync(db, PowerBi());

        // Ativa a venda do Power BI Essencial UMA ÚNICA VEZ. Fica guardado por
        // AtualizadoEm: depois de rodar (ou se o admin editar o curso), não mexe
        // mais — assim não atropela mudanças futuras feitas no painel.
        await AtivarVendaAsync(db, "power-bi-essencial", 990m);
    }

    /// <summary>Coloca o curso à venda com o preço informado, só se ele ainda não
    /// foi configurado manualmente (AtualizadoEm nulo). Idempotente.</summary>
    private static async Task AtivarVendaAsync(ApplicationDbContext db, string slug, decimal preco)
    {
        var curso = await db.CursosOnline.FirstOrDefaultAsync(c => c.Slug == slug);
        if (curso != null && curso.AtualizadoEm == null && !curso.AVenda)
        {
            curso.AVenda = true;
            curso.Preco = preco;
            curso.AtualizadoEm = DateTime.Now;
            await db.SaveChangesAsync();
        }
    }

    private static async Task GarantirAsync(ApplicationDbContext db, CursoOnline curso)
    {
        if (await db.CursosOnline.AnyAsync(c => c.Slug == curso.Slug)) return;
        db.CursosOnline.Add(curso);
        await db.SaveChangesAsync();
    }

    private static CursoOnline Mashup() => new CursoOnline
    {
        Nome = "QlikSense Mashup Developer",
        Slug = "qliksense-mashup-developer",
        DescricaoCurta = "Crie mashups do Qlik Sense do zero: do Dev Hub às APIs, montando um portal web completo (Site Auto MT) e publicando no Enterprise ou no QAP.",
        DescricaoCompleta = "Curso prático de desenvolvimento de mashups no Qlik Sense. Você aprende a preparar o ambiente, usar o Dev Hub, entender a anatomia de um mashup (qext, html, js e css) e programar por código: abrir o app, fazer seleções, criar gráficos, hipercubos, KPIs e filtros em HTML. Ao final, constrói um portal moderno (o Site Auto MT) e aprende a publicar no Qlik Sense Enterprise e no Qlik Analytics Platform (QAP).",
        CorCapa = "#0d9488",
        IconeCapa = "bi-code-slash",
        Categoria = "Desenvolvimento Qlik",
        Instrutor = "James Dias",
        Status = StatusCursoOnline.Publicado,
        Ordem = 3,
        CargaHoraria = 16,
        Certificado = true,
        LogosCertificado = "tds,qlik",
        Fabricante = FabricanteCertificado.Qlik,
        Preco = 990m,
        AVenda = false,
        Modulos = new List<ModuloCurso>
        {
            new ModuloCurso { Titulo = "Introdução e Ambiente", Ordem = 1, Licoes = new List<Licao>
            {
                new Licao { Titulo = "O que é um mashup, onde ele se encaixa e licenças", Ordem = 1, Duracao = 3, UrlVideoVimeo = "https://vimeo.com/1244176205" },
                new Licao { Titulo = "Onde baixar o Qlik Sense Desktop e os requisitos", Ordem = 2, Duracao = 3, UrlVideoVimeo = "https://vimeo.com/1244176376" },
                new Licao { Titulo = "Instalar, autenticar e preparar o ambiente", Ordem = 3, Duracao = 4, UrlVideoVimeo = "https://vimeo.com/1244176544" },
            }},
            new ModuloCurso { Titulo = "Dev Hub e Anatomia do Mashup", Ordem = 2, Licoes = new List<Licao>
            {
                new Licao { Titulo = "Dev Hub e o primeiro mashup pelo template", Ordem = 1, Duracao = 3, UrlVideoVimeo = "https://vimeo.com/1244176579" },
                new Licao { Titulo = "Anatomia do mashup: qext, html, js e css", Ordem = 2, Duracao = 3, UrlVideoVimeo = "https://vimeo.com/1244176698" },
            }},
            new ModuloCurso { Titulo = "Programando o Mashup por Código", Ordem = 3, Licoes = new List<Licao>
            {
                new Licao { Titulo = "Abrir o app e trazer objetos por código", Ordem = 1, Duracao = 2, UrlVideoVimeo = "https://vimeo.com/1244176729" },
                new Licao { Titulo = "Seleções por código", Ordem = 2, Duracao = 2, UrlVideoVimeo = "https://vimeo.com/1244176753" },
                new Licao { Titulo = "Gráficos criados por código", Ordem = 3, Duracao = 2, UrlVideoVimeo = "https://vimeo.com/1244176858" },
                new Licao { Titulo = "Hipercubos e listas: KPIs e filtros em HTML", Ordem = 4, Duracao = 3, UrlVideoVimeo = "https://vimeo.com/1244176915" },
            }},
            new ModuloCurso { Titulo = "Projeto, Depuração e Publicação", Ordem = 4, Licoes = new List<Licao>
            {
                new Licao { Titulo = "O template do Site Auto MT", Ordem = 1, Duracao = 3, UrlVideoVimeo = "https://vimeo.com/1244176957" },
                new Licao { Titulo = "Depuração: quando a página fica em branco", Ordem = 2, Duracao = 2, UrlVideoVimeo = "https://vimeo.com/1244177106" },
                new Licao { Titulo = "Publicando no Qlik Sense Enterprise e no QAP", Ordem = 3, Duracao = 3, UrlVideoVimeo = "https://vimeo.com/1244177171" },
                new Licao { Titulo = "Projeto final e próximos passos", Ordem = 4, Duracao = 3, UrlVideoVimeo = "https://vimeo.com/1244175483" },
            }},
            new ModuloCurso { Titulo = "Projeto Prático: Site Auto MT", Ordem = 5, Licoes = new List<Licao>
            {
                new Licao { Titulo = "Preparação, botões de ano e KPIs", Ordem = 1, Duracao = 3, UrlVideoVimeo = "https://vimeo.com/1244177309" },
                new Licao { Titulo = "Os gráficos do site", Ordem = 2, Duracao = 2, UrlVideoVimeo = "https://vimeo.com/1244177367" },
                new Licao { Titulo = "Chips de modelo e rank de vendas", Ordem = 3, Duracao = 2, UrlVideoVimeo = "https://vimeo.com/1244177413" },
                new Licao { Titulo = "Barra de seleções e conferência da entrega", Ordem = 4, Duracao = 3, UrlVideoVimeo = "https://vimeo.com/1244177468" },
            }},
        }
    };

    private static CursoOnline Lgpd() => new CursoOnline
    {
        Nome = "LGPD — Proteção de Dados Pessoais",
        Slug = "lgpd-protecao-de-dados",
        DescricaoCurta = "Entenda a Lei Geral de Proteção de Dados na prática — dos conceitos às obrigações — com preparação para a certificação EXIN Privacy & Data Protection.",
        DescricaoCompleta = "Formação completa em LGPD, da base à aplicação prática. Você percorre a história da privacidade, a quem a lei se aplica, os conceitos essenciais (dado pessoal, sensível e anonimizado), os papéis (titular, controlador, operador, encarregado e ANPD), os princípios, as bases legais, os direitos do titular, segurança e comunicação de incidentes, sanções, governança e boas práticas (inventário de dados, RIPD, políticas e contratos). O curso ainda prepara para a prova com um módulo de revisão e simulados, alinhado à certificação EXIN.",
        CorCapa = "#1d4ed8",
        IconeCapa = "bi-shield-lock-fill",
        Categoria = "Governança e Privacidade",
        Instrutor = "Daniel Côrtes",
        Status = StatusCursoOnline.Publicado,
        Ordem = 4,
        CargaHoraria = 16,
        Certificado = true,
        LogosCertificado = "tds",
        Fabricante = FabricanteCertificado.Tds,
        Preco = 990m,
        AVenda = false,
        Modulos = new List<ModuloCurso>
        {
            new ModuloCurso { Titulo = "Fundamentos e História da Privacidade", Ordem = 1, Licoes = new List<Licao>
            {
                new Licao { Titulo = "Boas-vindas: o curso, o instrutor e a certificação EXIN", Ordem = 1, Duracao = 4, UrlVideoVimeo = "https://vimeo.com/1244178474" },
                new Licao { Titulo = "A história da privacidade", Ordem = 2, Duracao = 3, UrlVideoVimeo = "https://vimeo.com/1244178514" },
                new Licao { Titulo = "A economia da informação", Ordem = 3, Duracao = 4, UrlVideoVimeo = "https://vimeo.com/1244178561" },
                new Licao { Titulo = "Regulamentação: do GDPR à LGPD", Ordem = 4, Duracao = 4, UrlVideoVimeo = "https://vimeo.com/1244178610" },
            }},
            new ModuloCurso { Titulo = "Aplicação da LGPD e Conceitos Essenciais", Ordem = 2, Licoes = new List<Licao>
            {
                new Licao { Titulo = "A quem a LGPD se aplica", Ordem = 1, Duracao = 3, UrlVideoVimeo = "https://vimeo.com/1244178809" },
                new Licao { Titulo = "Quando a LGPD não se aplica: as exceções", Ordem = 2, Duracao = 3, UrlVideoVimeo = "https://vimeo.com/1244178842" },
                new Licao { Titulo = "Conceitos essenciais: dado pessoal, sensível e anonimizado", Ordem = 3, Duracao = 4, UrlVideoVimeo = "https://vimeo.com/1244178900" },
                new Licao { Titulo = "Crianças, poder público e transferência internacional", Ordem = 4, Duracao = 4, UrlVideoVimeo = "https://vimeo.com/1244178941" },
            }},
            new ModuloCurso { Titulo = "Papéis, Princípios e Bases Legais", Ordem = 3, Licoes = new List<Licao>
            {
                new Licao { Titulo = "Quem é quem: titular, controlador, operador, encarregado e ANPD", Ordem = 1, Duracao = 3, UrlVideoVimeo = "https://vimeo.com/1244179141" },
                new Licao { Titulo = "Os princípios do tratamento de dados", Ordem = 2, Duracao = 4, UrlVideoVimeo = "https://vimeo.com/1244179181" },
                new Licao { Titulo = "As bases legais: quando o tratamento é permitido", Ordem = 3, Duracao = 4, UrlVideoVimeo = "https://vimeo.com/1244179248" },
            }},
            new ModuloCurso { Titulo = "Direitos do Titular", Ordem = 4, Licoes = new List<Licao>
            {
                new Licao { Titulo = "Os direitos do titular (art. 18)", Ordem = 1, Duracao = 4, UrlVideoVimeo = "https://vimeo.com/1244179298" },
                new Licao { Titulo = "Decisões automatizadas, portabilidade, oposição e revogação", Ordem = 2, Duracao = 3, UrlVideoVimeo = "https://vimeo.com/1244179344" },
            }},
            new ModuloCurso { Titulo = "Segurança, Incidentes e Sanções", Ordem = 5, Licoes = new List<Licao>
            {
                new Licao { Titulo = "Segurança da informação e incidentes", Ordem = 1, Duracao = 3, UrlVideoVimeo = "https://vimeo.com/1244179527" },
                new Licao { Titulo = "Comunicação de incidentes: o passo a passo", Ordem = 2, Duracao = 5, UrlVideoVimeo = "https://vimeo.com/1244179606" },
                new Licao { Titulo = "Sanções administrativas e a fiscalização da ANPD", Ordem = 3, Duracao = 4, UrlVideoVimeo = "https://vimeo.com/1244179662" },
            }},
            new ModuloCurso { Titulo = "Governança e Boas Práticas", Ordem = 6, Licoes = new List<Licao>
            {
                new Licao { Titulo = "Governança e privacidade desde a concepção", Ordem = 1, Duracao = 4, UrlVideoVimeo = "https://vimeo.com/1244179715" },
                new Licao { Titulo = "Inventário de dados e agentes de pequeno porte", Ordem = 2, Duracao = 4, UrlVideoVimeo = "https://vimeo.com/1244179837" },
                new Licao { Titulo = "Relatório de Impacto à Proteção de Dados (RIPD)", Ordem = 3, Duracao = 4, UrlVideoVimeo = "https://vimeo.com/1244179906" },
                new Licao { Titulo = "Consentimento, política de privacidade e contratos", Ordem = 4, Duracao = 4, UrlVideoVimeo = "https://vimeo.com/1244179977" },
                new Licao { Titulo = "Medidas técnicas, ciclo de vida e cultura", Ordem = 5, Duracao = 3, UrlVideoVimeo = "https://vimeo.com/1244180026" },
            }},
            new ModuloCurso { Titulo = "Preparação para a Certificação EXIN", Ordem = 7, Licoes = new List<Licao>
            {
                new Licao { Titulo = "Como é a prova e como responder", Ordem = 1, Duracao = 3, UrlVideoVimeo = "https://vimeo.com/1244178252" },
                new Licao { Titulo = "Simulado parte 1: escopo, conceitos e papéis", Ordem = 2, Duracao = 6, UrlVideoVimeo = "https://vimeo.com/1244180229" },
                new Licao { Titulo = "Simulado parte 2: princípios, bases legais e direitos", Ordem = 3, Duracao = 5, UrlVideoVimeo = "https://vimeo.com/1244180281" },
                new Licao { Titulo = "Simulado parte 3: segurança, sanções e governança", Ordem = 4, Duracao = 6, UrlVideoVimeo = "https://vimeo.com/1244180353" },
            }},
        }
    };

    private static CursoOnline PowerBi() => new CursoOnline
    {
        Nome = "Power BI Essencial",
        Slug = "power-bi-essencial",
        DescricaoCurta = "Formação prática em Microsoft Power BI: do zero ao dashboard profissional — Power Query (ETL), modelagem, DAX, visualizações e publicação no Power BI Service.",
        DescricaoCompleta = "Curso completo e prático de Microsoft Power BI, do zero ao dashboard profissional. Você começa entendendo o que é BI e a interface do Power BI Desktop; aprende a obter e preparar dados com o Power Query (ETL) e a linguagem M; modela os dados (modelo estrela, relacionamentos, tabela calendário); cria visualizações (gráficos, tabelas, mapas, visuais de IA e Copilot); domina cálculos com DAX (medidas, CALCULATE, inteligência de tempo, variáveis); e finaliza com um projeto completo publicado no Serviço do Power BI. Ideal para quem quer analisar e visualizar dados na prática e sair com um relatório de verdade no portfólio.",
        CorCapa = "#C99700",
        IconeCapa = "bi-bar-chart-fill",
        Categoria = "Business Intelligence",
        Instrutor = "Daniel Côrtes",
        Status = StatusCursoOnline.Publicado,
        Ordem = 5,
        CargaHoraria = 40,
        Certificado = true,
        LogosCertificado = "tds,microsoft",
        Fabricante = FabricanteCertificado.Microsoft,
        Preco = 990m,
        AVenda = false,
        Modulos = new List<ModuloCurso>
        {
            new ModuloCurso { Titulo = "Primeiros Passos no Power BI", Ordem = 1, Licoes = new List<Licao>
            {
                new Licao { Titulo = "O que é BI e onde o Power BI se encaixa", Ordem = 1, Duracao = 4, UrlVideoVimeo = "https://vimeo.com/1245234263" },
                new Licao { Titulo = "Instalando o Power BI Desktop e tour pela interface", Ordem = 2, Duracao = 4, UrlVideoVimeo = "https://vimeo.com/1245234296" },
                new Licao { Titulo = "Interatividade: filtragem cruzada, realce e segmentações", Ordem = 3, Duracao = 3, UrlVideoVimeo = "https://vimeo.com/1245234312" },
                new Licao { Titulo = "Explorando um relatório pronto: páginas, pesquisa e detalhamento", Ordem = 4, Duracao = 3, UrlVideoVimeo = "https://vimeo.com/1245234311" },
            }},
            new ModuloCurso { Titulo = "Carregando e Preparando Dados (Power Query)", Ordem = 2, Licoes = new List<Licao>
            {
                new Licao { Titulo = "Obter dados: importando um Excel", Ordem = 1, Duracao = 4, UrlVideoVimeo = "https://vimeo.com/1245234840" },
                new Licao { Titulo = "Editor do Power Query: perfil, tipos e etapas aplicadas", Ordem = 2, Duracao = 4, UrlVideoVimeo = "https://vimeo.com/1245234861" },
                new Licao { Titulo = "Transformações essenciais e a linguagem M", Ordem = 3, Duracao = 4, UrlVideoVimeo = "https://vimeo.com/1245234862" },
                new Licao { Titulo = "Atualizando dados e resolvendo erros de carga", Ordem = 4, Duracao = 4, UrlVideoVimeo = "https://vimeo.com/1245234889" },
                new Licao { Titulo = "Tabela Calendário: Ano, Mês e Trimestre", Ordem = 5, Duracao = 5, UrlVideoVimeo = "https://vimeo.com/1245231109" },
            }},
            new ModuloCurso { Titulo = "Modelagem de Dados", Ordem = 3, Licoes = new List<Licao>
            {
                new Licao { Titulo = "Exibição de Modelo: relacionamentos, cardinalidade e direção do filtro", Ordem = 1, Duracao = 4, UrlVideoVimeo = "https://vimeo.com/1245231108" },
                new Licao { Titulo = "Tabela fato e tabelas dimensão (modelo estrela)", Ordem = 2, Duracao = 3, UrlVideoVimeo = "https://vimeo.com/1245231106" },
                new Licao { Titulo = "Metas: tabelas com granularidade diferente", Ordem = 3, Duracao = 5, UrlVideoVimeo = "https://vimeo.com/1245231105" },
                new Licao { Titulo = "Relacionamentos inativos, muitos-para-muitos e filtro nos dois sentidos", Ordem = 4, Duracao = 3, UrlVideoVimeo = "https://vimeo.com/1245231442" },
            }},
            new ModuloCurso { Titulo = "Visualização de Dados", Ordem = 4, Licoes = new List<Licao>
            {
                new Licao { Titulo = "Páginas, tela, grade e boas práticas de layout", Ordem = 1, Duracao = 4, UrlVideoVimeo = "https://vimeo.com/1245231567" },
                new Licao { Titulo = "Cartão, gráfico de barras e de linhas", Ordem = 2, Duracao = 5, UrlVideoVimeo = "https://vimeo.com/1245231844" },
                new Licao { Titulo = "Tabela, matriz e segmentação de dados", Ordem = 3, Duracao = 4, UrlVideoVimeo = "https://vimeo.com/1245231902" },
                new Licao { Titulo = "Mapa, rosca e gráfico combinado: quando usar cada um", Ordem = 4, Duracao = 5, UrlVideoVimeo = "https://vimeo.com/1245231915" },
                new Licao { Titulo = "Visuais de IA: árvore de decomposição, principais influenciadores e Copilot", Ordem = 5, Duracao = 4, UrlVideoVimeo = "https://vimeo.com/1245231994" },
                new Licao { Titulo = "Temas, formatação condicional e formatação de números", Ordem = 6, Duracao = 3, UrlVideoVimeo = "https://vimeo.com/1245232602" },
            }},
            new ModuloCurso { Titulo = "DAX — Cálculos e Medidas", Ordem = 5, Licoes = new List<Licao>
            {
                new Licao { Titulo = "Medidas DAX: SUM, COUNTROWS, DISTINCTCOUNT, AVERAGE, MAX e MIN", Ordem = 1, Duracao = 4, UrlVideoVimeo = "https://vimeo.com/1245232607" },
                new Licao { Titulo = "Colunas calculadas, medidas e tabela de medidas", Ordem = 2, Duracao = 4, UrlVideoVimeo = "https://vimeo.com/1245232704" },
                new Licao { Titulo = "Variáveis, IF, SWITCH e parâmetros numéricos", Ordem = 3, Duracao = 5, UrlVideoVimeo = "https://vimeo.com/1245232760" },
                new Licao { Titulo = "CALCULATE e inteligência de tempo: ano atual vs ano anterior", Ordem = 4, Duracao = 4, UrlVideoVimeo = "https://vimeo.com/1245233162" },
                new Licao { Titulo = "Indicadores (bookmarks) e botões de navegação", Ordem = 5, Duracao = 3, UrlVideoVimeo = "https://vimeo.com/1245233239" },
            }},
            new ModuloCurso { Titulo = "Projeto Final e Publicação", Ordem = 6, Licoes = new List<Licao>
            {
                new Licao { Titulo = "Projeto final (parte 1): arquivo, tema e cabeçalho", Ordem = 1, Duracao = 4, UrlVideoVimeo = "https://vimeo.com/1245233304" },
                new Licao { Titulo = "Projeto final (parte 2): cartões e gráficos com design moderno", Ordem = 2, Duracao = 4, UrlVideoVimeo = "https://vimeo.com/1245233556" },
                new Licao { Titulo = "Projeto final (parte 3): página Vendedores, navegação e filtros sincronizados", Ordem = 3, Duracao = 4, UrlVideoVimeo = "https://vimeo.com/1245233705" },
                new Licao { Titulo = "Publicando no Serviço do Power BI: workspace, assinatura e atualização", Ordem = 4, Duracao = 3, UrlVideoVimeo = "https://vimeo.com/1245233796" },
                new Licao { Titulo = "Exportação, modelo PBIT e entrega do projeto", Ordem = 5, Duracao = 2, UrlVideoVimeo = "https://vimeo.com/1245233979" },
                new Licao { Titulo = "Encerramento e próximos passos", Ordem = 6, Duracao = 1, UrlVideoVimeo = "https://vimeo.com/1245234184" },
            }},
        }
    };
}
