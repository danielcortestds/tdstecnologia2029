using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TdsTecnologia2027.Web.Models;

namespace TdsTecnologia2027.Web.Data;

public static class SeedData
{
    public static async Task InitializeAsync(IServiceProvider serviceProvider, IConfiguration configuration)
    {
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();
        var db = serviceProvider.GetRequiredService<ApplicationDbContext>();

        // 1) Criar os papéis (roles) de acesso ao admin:
        //    Master = acesso total | Editor = somente cursos
        foreach (var papel in new[] { "Master", "Editor", "Aluno" })
        {
            if (!await roleManager.RoleExistsAsync(papel))
                await roleManager.CreateAsync(new IdentityRole(papel));
        }

        // 2) Criar usuário admin inicial se não existir
        var adminEmail = configuration["AdminSeed:Email"] ?? "admin@tdstecnologia.com.br";
        var adminSenha = configuration["AdminSeed:Senha"] ?? "Admin@123";

        var adminExistente = await userManager.FindByEmailAsync(adminEmail);
        if (adminExistente == null)
        {
            adminExistente = new IdentityUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true
            };
            await userManager.CreateAsync(adminExistente, adminSenha);
        }

        // 3) Garante que o admin inicial seja Master (inclusive em instalações
        //    que vinham do papel antigo "Admin").
        if (adminExistente != null && !await userManager.IsInRoleAsync(adminExistente, "Master"))
            await userManager.AddToRoleAsync(adminExistente, "Master");

        // 3.1) Limpeza do papel legado "Admin": migra todos os usuários que ainda
        //      o possuem para "Master" e remove o papel antigo do sistema.
        if (await roleManager.RoleExistsAsync("Admin"))
        {
            var antigos = await userManager.GetUsersInRoleAsync("Admin");
            foreach (var u in antigos)
            {
                if (!await userManager.IsInRoleAsync(u, "Master"))
                    await userManager.AddToRoleAsync(u, "Master");
                await userManager.RemoveFromRoleAsync(u, "Admin");
            }
            var roleAdmin = await roleManager.FindByNameAsync("Admin");
            if (roleAdmin != null)
                await roleManager.DeleteAsync(roleAdmin);
        }

        // 4) Popular cursos iniciais (apenas se a tabela estiver vazia)
        if (!await db.Cursos.AnyAsync())
        {
            var cursos = new List<Curso>
            {
                new()
                {
                    Slug = "powerbi",
                    Titulo = "Power BI Essencial",
                    Categoria = "Análise de Dados",
                    Icone = "fa-solid fa-chart-bar",
                    DescricaoCurta = "Domine a ferramenta de BI mais usada nas empresas. Dashboards profissionais, DAX e relatórios para decisões estratégicas.",
                    Descricao = "O Power BI é a ferramenta de Business Intelligence mais utilizada nas empresas do mundo. Você aprende do zero a construir dashboards profissionais, trabalhar com DAX, conectar fontes de dados e gerar relatórios que suportam decisões estratégicas.",
                    CargaHoraria = 40,
                    Preco = 1490.00m,
                    DataInicio = new DateTime(2026, 5, 21),
                    PillSecundaria = "10x sem juros",
                    IconePillSecundaria = "fa-solid fa-credit-card",
                    Topicos = string.Join('\n', new[]
                    {
                        "Construção de dashboards interativos e profissionais",
                        "Linguagem DAX — medidas, colunas calculadas e funções avançadas",
                        "Conexão com Excel, SQL, APIs e Azure",
                        "Publicação e compartilhamento no Power BI Service",
                        "Projeto prático com dados reais de negócio",
                        "Certificado oficial Microsoft ao concluir"
                    }),
                    WhatsAppMensagem = "Olá, quero saber mais sobre o Power BI Essencial!",
                    Ordem = 1,
                    Ativo = true
                },
                new()
                {
                    Slug = "python",
                    Titulo = "Python para Análise de Dados Aplicada",
                    Categoria = "Python & Dados",
                    Icone = "fa-brands fa-python",
                    DescricaoCurta = "Pandas, NumPy, Matplotlib e Scikit-learn aplicados a casos reais de negócio. Do zero à análise preditiva com Python.",
                    Descricao = "Python é a linguagem mais demandada pelo mercado de dados. Você aprende desde a sintaxe até análise exploratória, visualização de dados e introdução ao machine learning — com projetos reais do início ao fim.",
                    CargaHoraria = 60,
                    Preco = 1990.00m,
                    DataInicio = new DateTime(2026, 5, 28),
                    PillSecundaria = "10x sem juros",
                    IconePillSecundaria = "fa-solid fa-credit-card",
                    Topicos = string.Join('\n', new[]
                    {
                        "Python do zero — sintaxe, estruturas e lógica de programação",
                        "Análise e manipulação de dados com Pandas e NumPy",
                        "Visualização de dados com Matplotlib e Seaborn",
                        "Introdução ao Machine Learning com Scikit-learn",
                        "Automação de relatórios e pipelines de dados",
                        "Certificado oficial Microsoft ao concluir"
                    }),
                    WhatsAppMensagem = "Olá, quero saber mais sobre Python para Análise de Dados!",
                    Ordem = 2,
                    Ativo = true
                },
                new()
                {
                    Slug = "agentes",
                    Titulo = "Dados & Agentes de IA",
                    Categoria = "Inteligência Artificial",
                    Icone = "fa-solid fa-robot",
                    DescricaoCurta = "Construa agentes inteligentes com dados reais. LLMs, automação e pipelines de IA para ambientes corporativos e governo.",
                    Descricao = "Aprenda a construir agentes de inteligência artificial que trabalham com dados reais. O curso abrange LLMs, frameworks modernos de agentes e pipelines de IA aplicados a ambientes corporativos e órgãos de governo.",
                    CargaHoraria = 80,
                    Preco = 2990.00m,
                    DataInicio = new DateTime(2026, 6, 4),
                    PillSecundaria = "In Company",
                    IconePillSecundaria = "fa-solid fa-building",
                    Topicos = string.Join('\n', new[]
                    {
                        "Fundamentos de IA generativa e modelos de linguagem (LLMs)",
                        "Criação de agentes autônomos com frameworks modernos",
                        "Pipelines de dados para alimentar e orquestrar modelos de IA",
                        "Integração com APIs de IA — Azure AI e OpenAI",
                        "Casos de uso em ambientes corporativos e governo",
                        "Certificado oficial Microsoft ao concluir"
                    }),
                    WhatsAppMensagem = "Olá, quero saber mais sobre Dados e Agentes de IA!",
                    Ordem = 3,
                    Ativo = true
                },
                new()
                {
                    Slug = "engenharia",
                    Titulo = "Engenharia de Dados",
                    Categoria = "Engenharia & Cloud",
                    Icone = "fa-solid fa-database",
                    DescricaoCurta = "Azure Data Factory, Databricks, ETL/ELT e arquiteturas modernas de dados. Formação avançada para engenheiros de dados.",
                    Descricao = "Formação avançada para quem quer projetar e manter arquiteturas modernas de dados. Você trabalha com as principais ferramentas do ecossistema Microsoft Azure para construir pipelines robustos e escaláveis.",
                    CargaHoraria = 80,
                    Preco = 3490.00m,
                    DataInicio = new DateTime(2026, 6, 11),
                    PillSecundaria = "In Company",
                    IconePillSecundaria = "fa-solid fa-building",
                    Topicos = string.Join('\n', new[]
                    {
                        "Arquitetura moderna de dados — Data Lake, Warehouse e Lakehouse",
                        "Pipelines ETL/ELT com Azure Data Factory",
                        "Processamento de big data com Databricks e Apache Spark",
                        "Orquestração e monitoramento com Azure Synapse Analytics",
                        "Modelagem de dados e governança corporativa",
                        "Certificado oficial Microsoft ao concluir"
                    }),
                    WhatsAppMensagem = "Olá, quero saber mais sobre Engenharia de Dados!",
                    Ordem = 4,
                    Ativo = true
                },
                new()
                {
                    Slug = "lgpd",
                    Titulo = "LGPD — Privacidade e Proteção de Dados",
                    Categoria = "Compliance",
                    Icone = "fa-solid fa-shield-halved",
                    DescricaoCurta = "Conformidade legal, gestão de dados pessoais, DPO e implementação da LGPD em empresas e órgãos públicos do MT.",
                    Descricao = "Compreenda e aplique a Lei Geral de Proteção de Dados na prática. Formação ideal para gestores, jurídico, RH e TI que precisam adequar suas organizações à legislação e às melhores práticas de privacidade.",
                    CargaHoraria = 32,
                    Preco = 1290.00m,
                    DataInicio = new DateTime(2026, 6, 18),
                    PillSecundaria = "In Company",
                    IconePillSecundaria = "fa-solid fa-building",
                    Topicos = string.Join('\n', new[]
                    {
                        "Fundamentos da LGPD e direitos dos titulares de dados",
                        "Papel e atribuições do DPO (Encarregado de Dados)",
                        "Mapeamento e inventário de dados pessoais da organização",
                        "Gestão de incidentes de segurança e notificações à ANPD",
                        "Implementação de compliance em empresas e órgãos públicos",
                        "Certificado de conclusão ao encerrar o curso"
                    }),
                    WhatsAppMensagem = "Olá, quero saber mais sobre LGPD Privacidade e Proteção de Dados!",
                    Ordem = 5,
                    Ativo = true
                }
            };

            db.Cursos.AddRange(cursos);
            await db.SaveChangesAsync();
        }

        // 5) Cursos adicionais idempotentes (inseridos uma única vez, por slug),
        //    mesmo quando a tabela já tem dados.
        await GarantirCursoAsync(db, new Curso
        {
            Slug = "qlik-sense",
            Titulo = "Qlik Sense Design e Developer",
            Categoria = "Business Intelligence",
            Icone = "fa-solid fa-chart-pie",
            DescricaoCurta = "Formação completa em Qlik Sense, a plataforma de BI que transforma dados em decisões. Da carga de dados (ETL) à criação de dashboards interativos — do básico ao avançado.",
            Descricao = "O Qlik Sense é uma das principais plataformas de Business Intelligence do mundo, usada para explorar dados e criar dashboards interativos que apoiam a tomada de decisão. Nesta formação você percorre todo o ciclo de desenvolvimento de aplicações de dados: da captação e preparação (ETL) ao design das visualizações. A trilha Design ensina a montar dashboards, gráficos e histórias de dados; a trilha Developer aprofunda em carga avançada, modelagem, performance e funções de script. Pré-requisito: conhecimento básico de SQL e noções de banco de dados. Ideal para profissionais de TI e usuários que querem criar aplicações com o Qlik Sense.",
            CargaHoraria = 20,
            Preco = 3000.00m,
            DataInicio = new DateTime(2026, 7, 14),
            PillSecundaria = "10x sem juros",
            IconePillSecundaria = "fa-solid fa-credit-card",
            Topicos = string.Join('\n', new[]
            {
                "Fundamentos de Business Intelligence e da plataforma Qlik Sense",
                "Conexão e carga de dados (ETL): conectores, scripts e associação de tabelas",
                "Criação de dashboards: gráficos, tabelas, mapas e indicadores",
                "Itens mestre, Storytelling e apresentações orientadas à decisão",
                "Carga avançada de dados: incremental, particionada e arquitetura de dados",
                "Modelagem e performance: LinkTable, modelos de associação e otimização",
                "Script avançado e funções essenciais (AGGR, mapping, peek, crosstable...)",
                "Projeto prático aplicando todo o ciclo — do dado ao dashboard",
                "Aulas às terças e quintas, das 19h às 22h — presencial ou online ao vivo",
                "Certificado de conclusão ao final do curso"
            }),
            WhatsAppMensagem = "Olá, quero saber mais sobre a Formação Qlik Sense Design e Developer!",
            Ordem = 6,
            Ativo = true
        });

        // 6) Atualização (uma vez) do conteúdo do curso de Power BI conforme a
        //    ementa oficial. O marcador "BI de autoatendimento" garante que só
        //    rode uma vez, preservando edições futuras feitas pelo painel admin.
        var powerbi = await db.Cursos.FirstOrDefaultAsync(c => c.Slug == "powerbi");
        if (powerbi != null && !(powerbi.Descricao ?? string.Empty).Contains("BI de autoatendimento"))
        {
            powerbi.DescricaoCurta = "Formação oficial Microsoft Power BI: o passo a passo para criar dashboards e relatórios dinâmicos. Do tratamento de dados (ETL/Power Query) ao DAX, modelagem e publicação no Power BI Service.";
            powerbi.Descricao = "Formação completa em Microsoft Power BI, a ferramenta de Business Intelligence mais usada no mercado. Você aprende o passo a passo para transformar dados em dashboards e relatórios dinâmicos que apoiam decisões. O curso une o módulo oficial Microsoft (Analisando Dados — BI de autoatendimento) aos módulos práticos da TDS: tratamento e modelagem de dados com Power Query (ETL), cálculos e medidas com DAX, criação de visualizações e relatórios interativos, e publicação, compartilhamento e segurança no Power BI Service. Pré-requisito: conhecimentos básicos de Excel e dados. Ideal para quem quer dominar análise e visualização de dados na prática.";
            powerbi.CargaHoraria = 30;
            powerbi.Topicos = string.Join('\n', new[]
            {
                "Fundamentos de BI de autoatendimento: análise e visualização de dados",
                "Conexão a fontes de dados: Excel, bancos de dados e o serviço Power BI",
                "Tratamento de dados com Power Query (ETL): moldar, combinar e padronizar tabelas",
                "Modelagem de dados e relacionamentos entre tabelas",
                "Cálculos e medidas com DAX, incluindo funções de data e tabela de datas",
                "Diferença entre linguagem M e DAX, colunas e medidas calculadas",
                "Criação de relatórios e visualizações interativas",
                "Publicação no Power BI Service: painéis, compartilhamento e favoritos",
                "Segurança em nível de linha, atualização automática e gateway de dados",
                "Certificado oficial Microsoft ao concluir"
            });
            powerbi.AtualizadoEm = DateTime.Now;
            await db.SaveChangesAsync();
        }

        // 7) Atualização (uma vez) do curso de Python conforme a ementa "Python
        //    Developer". Marcador "Kivy" garante execução única.
        var python = await db.Cursos.FirstOrDefaultAsync(c => c.Slug == "python");
        if (python != null && !(python.Descricao ?? string.Empty).Contains("Kivy"))
        {
            python.Titulo = "Python Developer";
            python.Categoria = "Programação Python";
            python.DescricaoCurta = "Formação Python Developer: do zero ao desenvolvimento prático. Fundamentos da linguagem, dados com Pandas/NumPy, apps com Kivy, automação web com Selenium e web scraping.";
            python.Descricao = "Python é uma das três linguagens mais populares do mundo, presente em desenvolvimento web, ciência de dados, automação e muito mais. Nesta formação Developer você aprende do zero ao prático: fundamentos da linguagem (variáveis, condições, loops e funções), estruturas de dados (listas, dicionários, strings, arquivos e APIs), análise de dados com Pandas, NumPy e gráficos com Matplotlib, criação de aplicativos com Kivy (mobile e desktop), automação web com Selenium (incluindo chatbot e envio de mensagens) e web scraping para extrair dados de páginas. Pré-requisito: lógica de programação e noções básicas de banco de dados. Ideal para quem quer programar em Python de forma versátil e aplicada.";
            python.CargaHoraria = 36;
            python.Topicos = string.Join('\n', new[]
            {
                "Fundamentos do Python: variáveis, operadores, condições (if/else) e loops",
                "Funções próprias, módulos, classes e bibliotecas",
                "Estruturas de dados: listas, dicionários e manipulação de strings",
                "Leitura de arquivos (txt, csv, xls, pdf), consumo de APIs e tratamento de erros",
                "Análise de dados com Pandas e NumPy + gráficos com Matplotlib",
                "Conexão com banco de dados e leitura de tabelas",
                "Criação de aplicativos com Kivy (mobile e desktop)",
                "Automação web com Selenium: formulários, chatbot e envio de mensagens",
                "Web scraping e crawler para extrair dados de páginas",
                "Automação de tarefas com PyAutoGUI (teclado, mouse e tela)",
                "Certificado de conclusão ao final do curso"
            });
            python.WhatsAppMensagem = "Olá, quero saber mais sobre a Formação Python Developer!";
            python.AtualizadoEm = DateTime.Now;
            await db.SaveChangesAsync();
        }
    }

    /// <summary>Insere um curso apenas se ainda não existir outro com o mesmo Slug.</summary>
    private static async Task GarantirCursoAsync(ApplicationDbContext db, Curso curso)
    {
        if (!await db.Cursos.AnyAsync(c => c.Slug == curso.Slug))
        {
            curso.CriadoEm = DateTime.Now;
            db.Cursos.Add(curso);
            await db.SaveChangesAsync();
        }
    }
}
