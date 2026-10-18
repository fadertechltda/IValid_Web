# 0007. Benefícios do cliente só na finalização do pedido

- **Status:** Aceita
- **Data:** 2026-10-13

## Contexto

Ao doar, o cliente ganha cashback, sobe de nível e acumula indicadores de impacto. Se esses benefícios fossem creditados na criação do pedido, cada cancelamento teria que desfazê-los, com risco de saldo negativo ou crédito em dobro.

## Decisão

Cashback ganho, contagem de doações e impacto só são creditados quando o pedido chega a `FINALIZADO`, dentro de uma transação, uma única vez (`beneficiosAplicados`). O cashback **usado** é debitado na criação e devolvido no cancelamento.

## Consequências

- Cancelar nunca precisa desfazer benefícios.
- O cliente só vê o cashback depois que a loja finaliza o pedido.
- A finalização depende da loja atualizar o status no painel.
