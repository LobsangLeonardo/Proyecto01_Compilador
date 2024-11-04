public class AnalizadorSintacticoAbs
{
    private List<Token> tokens;
    private int posicionActual;

    public AnalizadorSintacticoAbs(List<Token> tokens)
    {
        this.tokens = tokens;
        this.posicionActual = 0;
    }

    public NodoAST Analizar()
    {
        return AnalizarPrograma();
    }

    private NodoAST AnalizarPrograma()
    {
        NodoAST raiz = new NodoAST("PROGRAM");
        
        while (posicionActual < tokens.Count)
        {
            raiz.AgregarHijo(AnalizarDeclaracion());
        }

        return raiz;
    }

    private NodoAST AnalizarDeclaracion()
    {
        Token tokenActual = ObtenerTokenActual();

        if (tokenActual.Tipo == TipoToken.PalabraClave)
        {
            switch (tokenActual.Valor.ToLower())
            {
                case "if": return AnalizarIf();
                case "while": return AnalizarWhile();
                case "for": return AnalizarFor();
            }
        }
        return AnalizarExpresion();
    }

    private NodoAST AnalizarIf()
    {
        NodoAST nodoIf = new NodoAST("IF_STATEMENT");
        Avanzar();

        if (ConsumirSiCoincide(TipoToken.Delimitador, "("))
        {
            NodoAST condicion = new NodoAST("CONDI");
            condicion.AgregarHijo(AnalizarExpresion());
            nodoIf.AgregarHijo(condicion);
            ConsumirSiCoincide(TipoToken.Delimitador, ")");
        }

        if (ConsumirSiCoincide(TipoToken.Delimitador, "{"))
        {
            NodoAST cuerpo = new NodoAST("BODY");
            while (!ConsumirSiCoincide(TipoToken.Delimitador, "}") && posicionActual < tokens.Count)
            {
                cuerpo.AgregarHijo(AnalizarDeclaracion());
            }
            nodoIf.AgregarHijo(cuerpo);
        }

        if (ConsumirSiCoincide(TipoToken.PalabraClave, "else"))
        {
            NodoAST nodoElse = new NodoAST("ELSE_BODY");
            if (ConsumirSiCoincide(TipoToken.Delimitador, "{"))
            {
                while (!ConsumirSiCoincide(TipoToken.Delimitador, "}") && posicionActual < tokens.Count)
                {
                    nodoElse.AgregarHijo(AnalizarDeclaracion());
                }
            }
            nodoIf.AgregarHijo(nodoElse);
        }

        return nodoIf;
    }

    private NodoAST AnalizarWhile()
    {
        NodoAST nodoWhile = new NodoAST("WHILE_STATEMENT");
        Avanzar();

        if (ConsumirSiCoincide(TipoToken.Delimitador, "("))
        {
            NodoAST condicion = new NodoAST("CONDI");
            condicion.AgregarHijo(AnalizarExpresion());
            nodoWhile.AgregarHijo(condicion);
            ConsumirSiCoincide(TipoToken.Delimitador, ")");
        }

        if (ConsumirSiCoincide(TipoToken.Delimitador, "{"))
        {
            NodoAST cuerpo = new NodoAST("BODY");
            while (!ConsumirSiCoincide(TipoToken.Delimitador, "}") && posicionActual < tokens.Count)
            {
                cuerpo.AgregarHijo(AnalizarDeclaracion());
            }
            nodoWhile.AgregarHijo(cuerpo);
        }

        return nodoWhile;
    }

    private NodoAST AnalizarExpresion()
    {
        NodoAST nodoExpresion = AnalizarTermino();

        while (posicionActual < tokens.Count && 
               ObtenerTokenActual().Tipo == TipoToken.Operador)
        {
            Avanzar();
            NodoAST nodoOperador = new NodoAST("OP");
            nodoOperador.AgregarHijo(nodoExpresion);
            nodoOperador.AgregarHijo(AnalizarTermino());
            nodoExpresion = nodoOperador;
        }

        return nodoExpresion;
    }

    private NodoAST AnalizarTermino()
    {
        Token token = ObtenerTokenActual();
        Avanzar();

        switch (token.Tipo)
        {
            case TipoToken.Numero: return new NodoAST("NUM");
            case TipoToken.Identificador: return new NodoAST("ID");
            case TipoToken.Cadena: return new NodoAST("STRING");
            default: return new NodoAST("TERM");
        }
    }

    private NodoAST AnalizarFor()
    {
        NodoAST nodoFor = new NodoAST("FOR_STATEMENT");
        Avanzar();

        if (ConsumirSiCoincide(TipoToken.Delimitador, "("))
        {
            NodoAST inicializacion = new NodoAST("INIT");
            if (!ConsumirSiCoincide(TipoToken.Delimitador, ";"))
            {
                inicializacion.AgregarHijo(AnalizarExpresion());
                ConsumirSiCoincide(TipoToken.Delimitador, ";");
            }
            nodoFor.AgregarHijo(inicializacion);

            NodoAST condicion = new NodoAST("COND");
            if (!ConsumirSiCoincide(TipoToken.Delimitador, ";"))
            {
                condicion.AgregarHijo(AnalizarExpresion());
                ConsumirSiCoincide(TipoToken.Delimitador, ";");
            }
            nodoFor.AgregarHijo(condicion);

            NodoAST incremento = new NodoAST("INCRE");
            if (!ConsumirSiCoincide(TipoToken.Delimitador, ")"))
            {
                incremento.AgregarHijo(AnalizarExpresion());
                ConsumirSiCoincide(TipoToken.Delimitador, ")");
            }
            nodoFor.AgregarHijo(incremento);
        }

        if (ConsumirSiCoincide(TipoToken.Delimitador, "{"))
        {
            NodoAST cuerpo = new NodoAST("BODY");
            while (!ConsumirSiCoincide(TipoToken.Delimitador, "}") && posicionActual < tokens.Count)
            {
                cuerpo.AgregarHijo(AnalizarDeclaracion());
            }
            nodoFor.AgregarHijo(cuerpo);
        }

        return nodoFor;
    }

    private Token ObtenerTokenActual()
    {
        if (posicionActual >= tokens.Count)
        {
            return new Token(TipoToken.Delimitador, "EOF", -1);
        }
        return tokens[posicionActual];
    }

    private void Avanzar()
    {
        posicionActual++;
    }

    private bool ConsumirSiCoincide(TipoToken tipo, string valor)
    {
        Token token = ObtenerTokenActual();
        if (token.Tipo == tipo && token.Valor == valor)
        {
            Avanzar();
            return true;
        }
        return false;
    }
}