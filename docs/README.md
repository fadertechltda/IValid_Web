# Documentação do IValid

Esta pasta reúne a documentação técnica do **IValid**: painel web (`WEB`) e API (`WEB_API`). O aplicativo Flutter fica em outro repositório e aparece aqui só como consumidor da API.

Se você acabou de chegar no projeto, leia nesta ordem:

1. [Visão geral](01-visao-geral.md): o que o sistema faz, quem usa e o vocabulário do domínio.
2. [Como rodar localmente](03-como-rodar-localmente.md): do clone até o painel aberto no navegador.
3. [Arquitetura](02-arquitetura.md): como as peças se encaixam.
4. [Guia de contribuição](11-guia-de-contribuicao.md): convenções e o passo a passo para criar uma funcionalidade nova.

## Como esta documentação está organizada

A organização segue duas referências usadas no mercado:

- **[Diátaxis](https://diataxis.fr/)**: separa o conteúdo pelo que o leitor precisa naquele momento. Tutorial para aprender, guia prático para resolver uma tarefa, referência para consultar e explicação para entender o porquê.
- **[arc42](https://arc42.org/)** com diagramas no estilo **[C4](https://c4model.com/)**: estrutura a documentação de arquitetura em contexto, containers, componentes, decisões e riscos.

As decisões importantes ficam registradas como **ADRs** (*Architecture Decision Records*), no formato de Michael Nygard: contexto, decisão e consequências. Assim, quem chegar depois entende por que algo foi feito de um jeito e não só como.

Os diagramas são escritos em [Mermaid](https://mermaid.js.org/), que o GitHub e o Visual Studio Code renderizam direto do Markdown. Por isso a documentação fica junto do código e é revisada no mesmo pull request que muda o comportamento (*docs as code*).

## Índice

| Documento | Tipo | Conteúdo |
|---|---|---|
| [01 Visão geral](01-visao-geral.md) | Explicação | Objetivo, atores, escopo e glossário |
| [02 Arquitetura](02-arquitetura.md) | Explicação | Diagramas C4, camadas, fluxo de uma requisição |
| [03 Como rodar localmente](03-como-rodar-localmente.md) | Tutorial | Pré-requisitos, configuração e primeiro acesso |
| [04 Configuração](04-configuracao.md) | Referência | Todas as chaves de `appsettings` e variáveis de ambiente |
| [05 Referência da API](05-api-referencia.md) | Referência | Todas as rotas, autenticação, payloads e erros |
| [06 Painel web](06-painel-web.md) | Referência | Telas, login, perfis, permissões e portal da plataforma |
| [07 Regras de negócio](07-regras-de-negocio.md) | Referência | Validade, descontos, pedidos, cashback, fidelidade e rotinas |
| [08 Modelo de dados](08-modelo-de-dados.md) | Referência | Coleções do Firestore e campos |
| [09 Segurança](09-seguranca.md) | Explicação | Autenticação, autorização, segredos e riscos |
| [10 Deploy](10-deploy.md) | Guia prático | Docker, Render e checklist de publicação |
| [11 Guia de contribuição](11-guia-de-contribuicao.md) | Guia prático | Convenções e como adicionar uma funcionalidade |
| [12 Problemas conhecidos](12-problemas-conhecidos.md) | Referência | Dívida técnica e pontos de atenção |
| [ADRs](adr/README.md) | Decisões | Por que o sistema é como é |

## Como manter

- Mudou uma rota, uma regra ou uma configuração? Atualize o documento correspondente no mesmo commit.
- Tomou uma decisão que alguém poderia questionar no futuro? Crie um ADR novo em `docs/adr/`. Não edite ADRs antigos: se a decisão mudou, crie outro e marque o antigo como "Substituído por".
- Use os nomes reais do código (classes, rotas, campos do Firestore) entre crases, para facilitar a busca.
