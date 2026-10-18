# 0001. Firestore como banco de dados

- **Status:** Aceita
- **Data:** não registrada (início do projeto)

## Contexto

O aplicativo Flutter já usava o Firebase (Authentication e Firestore) e gravava produtos e pedidos direto no banco. O projeto é acadêmico, com equipe pequena e sem orçamento para servidor de banco.

## Decisão

Usar o Firestore como único banco, acessado pela API com uma conta de serviço por meio do `Google.Cloud.Firestore`. Todo acesso fica na camada `REPOSITORY`.

## Consequências

- Sem servidor de banco para manter e com plano gratuito generoso.
- Sem esquema nem chaves estrangeiras: a consistência é responsabilidade da API (por exemplo, `storeName` copiado nos produtos).
- Nomes de campos em inglês herdados do app precisam ser mantidos.
- Consultas são limitadas (filtros simples, índices compostos sob demanda). Algumas telas acabam filtrando em memória.
- Operações que mexem em vários documentos usam transações (`TransacaoMapeador`).
