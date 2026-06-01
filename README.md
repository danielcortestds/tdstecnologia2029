# TDS Tecnologia 2027 — Site Institucional

Site institucional da TDS Tecnologia construído com **ASP.NET Core 8.0 MVC + Razor Views**, com área administrativa protegida por autenticação.

---

## Pré-requisitos

| Ferramenta | Versão mínima |
|---|---|
| .NET SDK | 8.0 |
| MySQL Server | 8.0 |
| dotnet-ef (global tool) | 8.x |

Instalar o EF Core tools global (se ainda não tiver):

```bash
dotnet tool install --global dotnet-ef
```

---

## Configuração do banco de dados

A connection string está em `src/TdsTecnologia2027.Web/appsettings.json`:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost;Database=tdstecnologia2027;User=root;Password=;AllowPublicKeyRetrieval=True;"
}
```

Ajuste `User` e `Password` conforme sua instalação local do MySQL.

---

## Como rodar

```bash
# 1. Restaurar pacotes
dotnet restore

# 2. Aplicar migrations (cria o banco automaticamente se não existir)
cd src/TdsTecnologia2027.Web
dotnet ef database update

# 3. Rodar a aplicação
dotnet run
```

A aplicação estará disponível em `https://localhost:xxxx/` (a porta é exibida no terminal).

> **Nota:** O banco é criado automaticamente via `Database.Migrate()` no `Program.cs`. O `dotnet ef database update` é necessário apenas em atualizações de schema sem reiniciar a aplicação.

---

## Credenciais do administrador seed

| Campo | Valor |
|---|---|
| E-mail | `admin@tdstecnologia.com.br` |
| Senha | `Admin@123` |

A senha está em `appsettings.Development.json` (`AdminSeed:Senha`). Altere antes de qualquer deploy.

---

## Área administrativa

- URL: `/Admin/Cursos`
- Login: `/Identity/Account/Login`
- Funcionalidades: CRUD completo de cursos com paginação, busca por título e soft delete.

---

## Observações sobre os assets do template

Os arquivos em `wwwroot/` (`style.css`, `css/`, `js/`, `images/`, `demos/chatbot/`) foram **copiados integralmente** de:

```
modelo-canva/.../Canvas 7 Files/demo-chatbot.html
```

**Não altere esses arquivos sem revisão do time de design.** Qualquer modificação visual deve ser feita em `wwwroot/css/custom.css` para evitar conflitos com futuras atualizações do template Canvas.
