# 0004. Perfis de permissão configuráveis por loja

- **Status:** Aceita
- **Data:** não registrada

## Contexto

No início, os papéis Gerente, Operador de Estoque e Atendente eram fixos no código (`PerfilFuncionario`). Qualquer ajuste de acesso exigia mudar o código e valia para todas as lojas.

## Decisão

Criar a coleção `perfis`, com um conjunto por loja. Cada perfil tem uma lista de permissões (`produtos`, `pedidos`, `financeiro`, `configuracoes`). No login, as permissões viram claims, e cada permissão é uma *policy* do ASP.NET Core. Os três papéis antigos viram perfis padrão, criados na primeira vez, e funcionários antigos são ligados a eles pelo campo de texto `perfil`.

## Consequências

- Lojas criam os próprios perfis sem mudança de código.
- Mudanças de perfil só valem no próximo login, porque ficam no cookie.
- Permissão nova exige só uma linha em `PermissaoUtil`.
- O código mantém a compatibilidade com o campo legado `perfil` até todos os funcionários serem editados.
