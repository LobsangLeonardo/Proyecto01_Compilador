public class GeneradorCodigoIntermedio
{
    private List<Token> tokens;
    private List<string> triplos;
    private int etiquetaActual;
    private int temporalActual;

    public GeneradorCodigoIntermedio(List<Token> tokens)
    {
        this.tokens = tokens;
        this.triplos = new List<string>();
        this.etiquetaActual = 0;
        this.temporalActual = 0;
    }

    public List<string> GenerarTriplos(NodoAST raiz)
    {
        Recorrer(raiz);
        return triplos;
    }

    private void Recorrer(NodoAST nodo)
    {
        if (nodo == null) return;

        switch (nodo.Tipo)
        {
            case "Programa":
                foreach (var hijo in nodo.Hijos)
                {
                    Recorrer(hijo);
                }
                break;

            case "if":
                GenerarCodigoIf(nodo);
                break;

            case "while":
                GenerarCodigoWhile(nodo);
                break;

            case "for":
                GenerarCodigoFor(nodo);
                break;

            case "=":
                GenerarCodigoAsignacion(nodo);
                break;

            case "return":
                GenerarCodigoReturn(nodo);
                break;

            default:
                if (nodo.Hijos.Count > 0)
                {
                    foreach (var hijo in nodo.Hijos)
                    {
                        Recorrer(hijo);
                    }
                }
                break;
        }
    }

    private string GenerarTemporal()
    {
        return $"t{temporalActual++}";
    }

    private string GenerarEtiqueta()
    {
        return $"L{etiquetaActual++}";
    }

    private string EvaluarExpresion(NodoAST nodo)
    {
        if (nodo.Hijos.Count == 0)
        {
            return nodo.Tipo; 
        }


        if (nodo.Hijos.Count == 2)
        {
            string izquierdo = EvaluarExpresion(nodo.Hijos[0]);
            string derecho = EvaluarExpresion(nodo.Hijos[1]);
            string temporal = GenerarTemporal();

        
            triplos.Add($"{nodo.Tipo}, {izquierdo}, {derecho}, {temporal}");
            return temporal;
        }

        return nodo.Tipo;
    }

    private void GenerarCodigoIf(NodoAST nodoIf)
    {
        
        string condicion = EvaluarExpresion(nodoIf.Hijos[0]);
        string etiquetaFalso = GenerarEtiqueta();
        string etiquetaSalida = GenerarEtiqueta();

        
        triplos.Add($"GOTO_FALSE, {condicion}, {etiquetaFalso}, -");

    
        Recorrer(nodoIf.Hijos[1]);

        
        if (nodoIf.Hijos.Count > 2)
        {
            triplos.Add($"GOTO, -, -, {etiquetaSalida}");
            triplos.Add($"ETIQUETA, {etiquetaFalso}, -, -");
            Recorrer(nodoIf.Hijos[2]);
            triplos.Add($"ETIQUETA, {etiquetaSalida}, -, -");
        }
        else
        {
            triplos.Add($"ETIQUETA, {etiquetaFalso}, -, -");
        }
    }

    private void GenerarCodigoWhile(NodoAST nodoWhile)
    {
        string etiquetaInicio = GenerarEtiqueta();
        string etiquetaSalida = GenerarEtiqueta();

        
        triplos.Add($"ETIQUETA, {etiquetaInicio}, -, -");

        
        string condicion = EvaluarExpresion(nodoWhile.Hijos[0]);
        
        
        triplos.Add($"GOTO_FALSE, {condicion}, {etiquetaSalida}, -");

        
        Recorrer(nodoWhile.Hijos[1]);

        
        triplos.Add($"GOTO, -, -, {etiquetaInicio}");

    
        triplos.Add($"ETIQUETA, {etiquetaSalida}, -, -");
    }

    private void GenerarCodigoFor(NodoAST nodoFor)
    {

        Recorrer(nodoFor.Hijos[0]);

        string etiquetaInicio = GenerarEtiqueta();
        string etiquetaSalida = GenerarEtiqueta();

        
        triplos.Add($"ETIQUETA, {etiquetaInicio}, -, -");

        
        string condicion = EvaluarExpresion(nodoFor.Hijos[1]);

        
        triplos.Add($"GOTO_FALSE, {condicion}, {etiquetaSalida}, -");

        
        Recorrer(nodoFor.Hijos[3]);

        
        Recorrer(nodoFor.Hijos[2]);

        
        triplos.Add($"GOTO, -, -, {etiquetaInicio}");

        
        triplos.Add($"ETIQUETA, {etiquetaSalida}, -, -");
    }

    private void GenerarCodigoAsignacion(NodoAST nodoAsignacion)
    {
        string variable = nodoAsignacion.Hijos[0].Tipo;
        string valor = EvaluarExpresion(nodoAsignacion.Hijos[1]);

        
        triplos.Add($"=, {valor}, -, {variable}");
    }

    private void GenerarCodigoReturn(NodoAST nodoReturn)
    {
        if (nodoReturn.Hijos.Count > 0)
        {
            string valorRetorno = EvaluarExpresion(nodoReturn.Hijos[0]);
            triplos.Add($"RETURN, {valorRetorno}, -, -");
        }
        else
        {
            triplos.Add("RETURN, -, -, -");
        }
    }

    public void ImprimirTriplos()
    {
        for (int i = 0; i < triplos.Count; i++)
        {
            Console.WriteLine($"{i}: {triplos[i]}");
        }
    }
}