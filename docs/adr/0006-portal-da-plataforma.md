# 0006. Portal da plataforma para dados compartilhados

- **Status:** Aceita
- **Data:** 2026-10-18

## Contexto

Categorias e ONGs são compartilhadas por todas as lojas, mas qualquer administrador de loja (ou funcionário com a permissão) podia criar, editar e excluir esses cadastros, afetando todas as outras lojas. Além disso, quem garante que uma ONG é legítima deve ser a equipe do IValid, não um participante.

## Decisão

- Criar o perfil `Plataforma`, fora de qualquer loja, com login próprio em `/Autenticacao/LoginPlataforma`.
- Autenticação pelo Firebase Auth e autorização por uma lista de e-mails em `Plataforma:EmailsAdministradores`.
- Categorias e ONGs deixam de ser permissões de loja e passam a ser exclusivas da plataforma. O portal também lista os supermercados.

Alternativas descartadas:
- *Custom claims* do Firebase: mais robusto, mas exige script para definir a claim e não traz ganho para o tamanho da equipe.
- Deixar a regra de desconto padrão editável na plataforma: o app tem o padrão fixo em Dart e os preços ficariam diferentes.

## Consequências

- Lojas não alteram mais dados de outras lojas.
- Adicionar alguém da equipe exige mudar configuração e reiniciar a API.
- Perfis antigos com as permissões `categorias` e `ongs` continuam funcionando, porque essas chaves são ignoradas.
