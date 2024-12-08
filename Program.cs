//-------------------Main principal-------------------
// Todos los  diferentes tipos de tokens
public enum TipoToken
{
    PalabraClave,
    Identificador,
    Operador,
    Numero,
    Delimitador,
    Cadena,
    Error,
    Desconocido,
    Comentario
}

// Clase para representar un token individual
public class Token
{
    public TipoToken Tipo { get; set; }
    public string Valor { get; set; }
    public int Linea { get; set; }

    public Token(TipoToken tipo, string valor, int linea)
    {
        Tipo = tipo;
        Valor = valor;
        Linea = linea;
    }

    public override string ToString()
    {
        return $"[{Tipo}] {Valor} (Línea: {Linea})";
    }
}

public class NodoAST
{
    public string Tipo { get; set; }
    public string Valor { get; set; }
    public List<NodoAST> Hijos { get; set; }

    public NodoAST(string tipo, string valor = "")
    {
        Tipo = tipo;
        Valor = valor;
        Hijos = new List<NodoAST>();
    }

    public void AgregarHijo(NodoAST hijo)
    {
        Hijos.Add(hijo);
    }

    public void Imprimir(string prefijo = "", bool esUltimo = true)
    {
        // Imprimir el nodo actual
        Console.WriteLine($"{prefijo}{(esUltimo ? "  \\ " : "  / ")}{Tipo}{(string.IsNullOrEmpty(Valor) ? "" : $" {Valor}")}");

        // Preparar el prefijo para los hijos
        for (int i = 0; i < Hijos.Count; i++)
        {
            Hijos[i].Imprimir(prefijo + (esUltimo ? "    " : " |  "), i == Hijos.Count - 1);
        }
    }
}

class Programa
{
    static void Main(string[] args)
    {
        // Solicitar al usuario el nombre del archivo de entrada
        Console.WriteLine("Coloque el nombre del archivo con su tipo:");
        string? rutaArchivo = Console.ReadLine();
        //En caso de un error y asi
        if (string.IsNullOrEmpty(rutaArchivo) || !File.Exists(rutaArchivo))
        {
            Console.WriteLine("Error: No se proporcionó un nombre de archivo válido o no se encuentra el archivo.");
            return;
        }

        string codigoFuente = File.ReadAllText(rutaArchivo);

        AnalizadorLexico analizadorLexico = new AnalizadorLexico(codigoFuente);
        List<Token> tokens = analizadorLexico.Analizar();
        //El menu para seleccionar  el cual ejecurarse
        Console.WriteLine("\nSeleccione una opción:");
        Console.WriteLine("1 - Mostrar la Tabla Léxica");
        Console.WriteLine("2 - Mostrar el Árbol Sintáctico");
        //Console.WriteLine("3 - Mostrar el Árbol Sintáctico Abstracto"); <--- para usar otro codigo del arbol
        Console.WriteLine("3 - Generar Código Intermedio");
        Console.WriteLine("4 - Generar Tabla de cuadruplos");
        string? opcion = Console.ReadLine();

    switch (opcion)
        {
            // Ejecutar la tabla de tokens
            case "1":
                MostrarTablaLexica(tokens);
                break;
            // Mostrar el arbol sintactico
            case "2":
                AnalizadorSintactico analizadorSintactico = new AnalizadorSintactico(tokens);
                NodoAST arbolSintactico = analizadorSintactico.Analizar();
                Console.WriteLine("\nÁrbol Sintáctico:");
                arbolSintactico.Imprimir();
                break;
            // Ejecutar el codigo intermedio con triplos
            case "3":   
                AnalizadorSintactico analizadorTriplos = new AnalizadorSintactico(tokens);
                NodoAST arbolTriplos = analizadorTriplos.Analizar();
                GeneradorCodigoIntermedio generadorTriplos = new GeneradorCodigoIntermedio(tokens);
                List<string> triplos = generadorTriplos.GenerarTriplos(arbolTriplos);

                Console.WriteLine("\nTriplos Generados(Codigo intermedio):");
                generadorTriplos.ImprimirTriplos();
                break;
            // Ejecutar la tabla de cuadruplos
            case "4":
                AnalizadorSintactico analizadorSintacticoCuadruplos = new AnalizadorSintactico(tokens);
                NodoAST arbolSintacticoCuadruplos = analizadorSintacticoCuadruplos.Analizar();
                GeneradorCuadruplos generadorCuadruplos = new GeneradorCuadruplos();
                List<Cuadruplo> cuadruplos = generadorCuadruplos.GenerarCuadruplos(arbolSintacticoCuadruplos);

                Console.WriteLine("\nCuádruplos Generados:");
                generadorCuadruplos.ImprimirCuadruplos();
                break;
            // Por si si ponen una opcion erronea
            default:
                Console.WriteLine("Opción no válida.");
                break;
        }
    }

    static void MostrarTablaLexica(List<Token> tokens)
    {
         // Definir anchos de columna para la salida
        const int tipoWidth = 15;
        const int valorWidth = 60;
        const int reservadaWidth = 10;
        const int lineaWidth = 10;

        // Imprimir encabezado de la tabl
        Console.WriteLine(new string('-', tipoWidth + valorWidth + reservadaWidth + lineaWidth + 6));
        Console.WriteLine($"|{"Tipo",-tipoWidth}|{"Token",-valorWidth}|{"Reservada",-reservadaWidth}|{"Línea",-lineaWidth}|");
        Console.WriteLine(new string('-', tipoWidth + valorWidth + reservadaWidth + lineaWidth + 6));

        // Imprimir tokens
        foreach (var token in tokens)
        {
            string reservada = token.Tipo == TipoToken.PalabraClave ? "Sí" : "No";
            Console.WriteLine($"|{token.Tipo,-tipoWidth}|{token.Valor,-valorWidth}|{reservada,-reservadaWidth}|{token.Linea,-lineaWidth}|");
        }
        // Imprimir pie de la tabla
        Console.WriteLine(new string('-', tipoWidth + valorWidth + reservadaWidth + lineaWidth + 6));
    }
    
}
