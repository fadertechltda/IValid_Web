using System;

namespace Excecoes
{
    public class IValidExcecao(CodigoExcecao codigo = CodigoExcecao.Generico, string? informacaoAdicional = null, Exception? excecaoInterna = null) : Exception(excecaoInterna?.Message, excecaoInterna)
    {
        public CodigoExcecao Codigo { get; } = codigo;

        public string? InformacaoAdicional { get; } = informacaoAdicional;
    }
}
