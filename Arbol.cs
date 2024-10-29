
// Analizador Sintáctico que desarrolla el árbol sintáctico abstracto.
public class AnalizadorSintactico
{
    private List<Token> tokens;
    private int posicionActual;

    
    // Inicializa una nueva instancia de la clase AnalizadorSintactico.
    public AnalizadorSintactico(List<Token> tokens)
    {
        this.tokens = tokens;
        this.posicionActual = 0;
    }

    
    // Analiza los tokens y construye el árbol sintáctico abstracto.
    public NodoAST Analizar()
    {
        NodoAST raiz = new NodoAST("");
        while (!FinDeTokens())
        {
            try
            {
                raiz.AgregarHijo(AnalizarDeclaracion());
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error en la línea {ObtenerTokenActual().Linea}: {e.Message}");
                Avanzar(); // Avanzar para evitar bucles infinitos en caso de error
            }
        }
        return raiz;
    }

    // Analiza una declaración y construye el nodo correspondiente en el arbol.
    private NodoAST AnalizarDeclaracion()
{
    Token token = ObtenerTokenActual();

    switch (token.Tipo)
    {
        case TipoToken.PalabraClave:
            switch (token.Valor.ToLower())
            {
                case "if":
                    return AnalizarIf(); // Analizar declaración if
                case "while":
                    return AnalizarWhile(); // Analizar declaración while
                case "for":
                    return AnalizarFor(); // Analizar declaración for
                case "return":
                    return AnalizarReturn(); // Analizar return
                default:
                    throw new Exception($"Palabra clave no reconocida: {token.Valor}");
            }
        case TipoToken.Identificador:
            return AnalizarAsignacion(); // Analizar asignación
        default:
            throw new Exception($"Token inesperado: {token.Valor}");
    }
}


    // Analiza una declaración de retorno.
    private NodoAST AnalizarReturn()
    {
        NodoAST nodoReturn = new NodoAST("return");
        Avanzar(); // Consumir 'return'
        
        if (!FinDeTokens() && ObtenerTokenActual().Tipo != TipoToken.Delimitador)
        {
            nodoReturn.AgregarHijo(AnalizarExpresion());
        }
        
        EsperarToken(TipoToken.Delimitador, ";");
        return nodoReturn;
    }

    // Analiza una declaración if y else??.
private NodoAST AnalizarIf()
{
    NodoAST nodoIf = new NodoAST("if");
    Avanzar(); // Consumir 'if'
    
    EsperarToken(TipoToken.Delimitador, "("); 
    nodoIf.AgregarHijo(AnalizarExpresion()); 
    EsperarToken(TipoToken.Delimitador, ")");
    nodoIf.AgregarHijo(AnalizarBloque()); 

    // Verificar si hay un 'else' inmediatamente después del bloque del if
    if (!FinDeTokens() && ObtenerTokenActual().Valor.ToLower() == "else")
    {
        Avanzar(); 
        
        // Crear un nodo 'else' y agregarle el bloque como hijo
        NodoAST nodoElse = new NodoAST("else");
        nodoElse.AgregarHijo(AnalizarBloque()); 
        
        nodoIf.AgregarHijo(nodoElse);
    }

    return nodoIf;
}

    // Analiza una declaración while.
private NodoAST AnalizarWhile()
{
    NodoAST nodoWhile = new NodoAST("while");
    Avanzar(); // Consumir 'while'

    EsperarToken(TipoToken.Delimitador, "("); 
    nodoWhile.AgregarHijo(AnalizarExpresion()); 
    EsperarToken(TipoToken.Delimitador, ")"); 

    nodoWhile.AgregarHijo(AnalizarBloque()); 

    return nodoWhile;
}


    // Analiza una declaración for.
private NodoAST AnalizarFor()
{
    NodoAST nodoFor = new NodoAST("for");
    Avanzar(); // Consumir 'for'
    EsperarToken(TipoToken.Delimitador, "("); 

    nodoFor.AgregarHijo(AnalizarAsignacion());

    EsperarToken(TipoToken.Delimitador, ";"); 
    nodoFor.AgregarHijo(AnalizarExpresion());

    EsperarToken(TipoToken.Delimitador, ";"); 
    nodoFor.AgregarHijo(AnalizarAsignacion());

    EsperarToken(TipoToken.Delimitador, ")"); 
    nodoFor.AgregarHijo(AnalizarBloque());

    return nodoFor;
}

    // Analiza una asignación.
    private NodoAST AnalizarAsignacion()
    {
        NodoAST nodoAsignacion = new NodoAST("=");
        nodoAsignacion.AgregarHijo(new NodoAST(ObtenerTokenActual().Valor));
        Avanzar(); // Consumir identificador
        EsperarToken(TipoToken.Operador, "=");
        nodoAsignacion.AgregarHijo(AnalizarExpresion());
        EsperarToken(TipoToken.Delimitador, ";");
        return nodoAsignacion;
    }

    // Analiza una expresión.
    private NodoAST AnalizarExpresion()
    {
        NodoAST nodoIzquierdo = new NodoAST(ObtenerTokenActual().Valor);
        Avanzar();
        
        if (!FinDeTokens() && ObtenerTokenActual().Tipo == TipoToken.Operador)
        {
            NodoAST nodoOperador = new NodoAST(ObtenerTokenActual().Valor);
            Avanzar();
            nodoOperador.AgregarHijo(nodoIzquierdo);
            nodoOperador.AgregarHijo(AnalizarExpresion());
            return nodoOperador;
        }
        
        return nodoIzquierdo;
    }

    
    // Analiza un bloque de código.
private NodoAST AnalizarBloque()
{
    NodoAST nodoBloque = new NodoAST(""); // Nodo sin etiqueta
    EsperarToken(TipoToken.Delimitador, "{"); 

    while (!FinDeTokens() && ObtenerTokenActual().Valor != "}")
    {
        nodoBloque.AgregarHijo(AnalizarDeclaracion()); 
    }

    EsperarToken(TipoToken.Delimitador, "}"); 
    return nodoBloque;
}

    // Obtiene el token actual sin avanzar la posición.
    private Token ObtenerTokenActual()
    {
        return FinDeTokens() ? new Token(TipoToken.Desconocido, "", -1) : tokens[posicionActual];
    }

    // Avanza la posición actual al siguiente token.
    private void Avanzar()
    {
        if (!FinDeTokens()) posicionActual++;
    }

    // Comprueba si se ha llegado al final de la lista de tokens.
    private bool FinDeTokens()
    {
        return posicionActual >= tokens.Count;
    }

    // Verifica si el token actual coincide con el tipo y valor esperados.
    private void EsperarToken(TipoToken tipo, string valor)
    {
        if (FinDeTokens() || ObtenerTokenActual().Tipo != tipo || ObtenerTokenActual().Valor != valor)
        {
            throw new Exception($"Se esperaba {tipo} '{valor}', pero se encontró {ObtenerTokenActual().Tipo} '{ObtenerTokenActual().Valor}'");
        }
        Avanzar();
    }
}
