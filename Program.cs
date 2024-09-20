using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

//---------------------------Ideas para agregar -----------------------------------------
// Todo bien por ahora,creo... 

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
    Desconocido     
}

// Clase para representar un token individual
public class Token
{
    public TipoToken Tipo { get; set; }  // Tipo del token
    public string Valor { get; set; }    // Valor del token
    public int Linea { get; set; }       // Número de linea donde esta el token

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

// La clase principal para el analisis lexico
public class AnalizadorLexico
{
    private string _codigoFuente;
    private int _posicion;
    private int _linea;
    private readonly List<string> _palabrasClave = new List<string> { "if", "else", "while", "return" };
    private readonly List<char> _operadores = new List<char> { '+', '-', '*', '/', '=', '!', '<', '>', '&', '|' };
    private readonly List<char> _delimitadores = new List<char> { '(', ')', '{', '}', ';', ',' };

    public AnalizadorLexico(string codigoFuente)
    {
        _codigoFuente = codigoFuente;
        _posicion = 0;
        _linea = 1;
    }

    // Metodo principal para realizar el analisis lexico
    public List<Token> Analizar()
    {
        List<Token> tokens = new List<Token>();

        while (_posicion < _codigoFuente.Length)
        {
            char caracterActual = _codigoFuente[_posicion];

            // Ignorar espacios en blanco y actualizar contador de lineas
            if (char.IsWhiteSpace(caracterActual))
            {
                if (caracterActual == '\n') _linea++;
                _posicion++;
                continue;
            }

            // Identificar palabras clave e identificadores
            if (char.IsLetter(caracterActual))
            {
                string identificador = LeerMientras(char.IsLetterOrDigit);
                if (_palabrasClave.Contains(identificador))
                {
                    tokens.Add(new Token(TipoToken.PalabraClave, identificador, _linea));
                }
                else
                {
                    tokens.Add(new Token(TipoToken.Identificador, identificador, _linea));
                }
            }
            // Identificar numeros
            else if (char.IsDigit(caracterActual) || caracterActual == '-')
            {
                string numero = LeerNumero();
                tokens.Add(new Token(TipoToken.Numero, numero, _linea));
            }
            // Identificar cadenas
            else if (caracterActual == '"')
            {
                _posicion++; 
                string cadena = LeerMientras(c => c != '"');

                if (_posicion < _codigoFuente.Length && _codigoFuente[_posicion] == '"')
                {
                    _posicion++; 
                    tokens.Add(new Token(TipoToken.Cadena, cadena, _linea));
                }
                else
                {
                    tokens.Add(new Token(TipoToken.Error, $"Cadena sin cerrar: \"{cadena}", _linea));
                }
            }
            // Identificar operadores y comentarios
            else if (_operadores.Contains(caracterActual))
            {
                if (caracterActual == '/' && VerProximo() == '/')
                {
                    _posicion += 2;
                    LeerMientras(c => c != '\n'); 
                }
                else if (caracterActual == '/' && VerProximo() == '*')
                {
                    _posicion += 2;
                    LeerHasta("*/");
                }
                else
                {
                    string operador = LeerMientras(c => _operadores.Contains(c));
                    tokens.Add(new Token(TipoToken.Operador, operador, _linea));
                }
            }
            // Identificar delimitadores
            else if (_delimitadores.Contains(caracterActual))
            {
                tokens.Add(new Token(TipoToken.Delimitador, caracterActual.ToString(), _linea));
                _posicion++;
            }
            // Manejar caracteres no reconocidos
            else
            {
                tokens.Add(new Token(TipoToken.Error, $"Carácter no reconocido: {caracterActual}", _linea));
                _posicion++;
            }
        }

        return tokens;
    }

    // Metodo auxiliar para ver el siguiente caracter sin avanzar la posicion
    private char VerProximo()
    {
        return _posicion + 1 < _codigoFuente.Length ? _codigoFuente[_posicion + 1] : '\0';
    }

    // Metodo auxiliar para leer caracteres mientras se cumpla una condicion
    private string LeerMientras(Func<char, bool> condicion)
    {
        StringBuilder resultado = new StringBuilder();
        while (_posicion < _codigoFuente.Length && condicion(_codigoFuente[_posicion]))
        {
            resultado.Append(_codigoFuente[_posicion]);
            _posicion++;
        }
        return resultado.ToString();
    }

    // Método auxiliar para leer hasta encontrar un delimitador especifico
    private string LeerHasta(string delimitador)
    {
        int indiceInicio = _posicion;
        while (_posicion < _codigoFuente.Length && !_codigoFuente.Substring(_posicion).StartsWith(delimitador))
        {
            _posicion++;
        }
        _posicion += delimitador.Length;
        return _codigoFuente.Substring(indiceInicio, _posicion - indiceInicio - delimitador.Length);
    }

    // Metodo auxiliar para leer y validar numeros
    private string LeerNumero()
    {
        bool esNegativo = _codigoFuente[_posicion] == '-';
        if (esNegativo) _posicion++;

        StringBuilder resultado = new StringBuilder(esNegativo ? "-" : "");
        bool puntoDecimalEncontrado = false;

        while (_posicion < _codigoFuente.Length)
        {
            char c = _codigoFuente[_posicion];
            if (char.IsDigit(c))
            {
                resultado.Append(c);
            }
            else if (c == '.' && !puntoDecimalEncontrado)
            {
                resultado.Append(c);
                puntoDecimalEncontrado = true;
            }
            else
            {
                break;
            }
            _posicion++;
        }

        string numeroStr = resultado.ToString();

        // Validar numeros de punto flotante
        if (puntoDecimalEncontrado)
        {
            if (double.TryParse(numeroStr, out double valorDouble))
            {
                if (valorDouble == double.PositiveInfinity || valorDouble == double.NegativeInfinity)
                {
                    return $"Error: Desbordamiento de punto flotante: {numeroStr}";
                }
            }
            else
            {
                return $"Error: Número de punto flotante inválido: {numeroStr}";
            }
        }
        // Validar enteros
        else
        {
            if (long.TryParse(numeroStr, out long valorLong))
            {
                if (valorLong > int.MaxValue || valorLong < int.MinValue)
                {
                    return $"Error: Desbordamiento de entero: {numeroStr}";
                }
            }
            else
            {
                return $"Error: Número entero inválido: {numeroStr}";
            }
        }

        return numeroStr;
    }
}

// Clase principal para imprimir
class Programa_Impri
{
    static void Main(string[] args)
    {
        // Solicitar al usuario el nombre del archivo de entrada
        Console.WriteLine("Coloque el nombre del archivo con su tipo:");
        string? rutaArchivo = Console.ReadLine();

        // Verificar si la entrada es nula o es vacia
        if (string.IsNullOrEmpty(rutaArchivo))
        {
            Console.WriteLine("Error: No se proporcionó un nombre de archivo válido.");
            return;
        }

        // Verificar si el archivo existe
        if (!File.Exists(rutaArchivo))
        {
            Console.WriteLine("Error: No se encuentra un archivo con ese nombre.");
            return;
        }

        // Leer el contenido del archivo
        string codigoFuente = File.ReadAllText(rutaArchivo);

        // Realizar el analisis lexico
        AnalizadorLexico analizador = new AnalizadorLexico(codigoFuente);
        List<Token> tokens = analizador.Analizar();

        // Definir anchos de columna para la salida
        const int tipoWidth = 15;
        const int valorWidth = 60;
        const int reservadaWidth = 10;
        const int lineaWidth = 10;

        // Imprimir encabezado de la tabla
        Console.WriteLine(new string('-', tipoWidth + valorWidth + reservadaWidth + lineaWidth + 6));
        Console.WriteLine($"|{"Tipo",-tipoWidth}|{"Valor",-valorWidth}|{"Reservada",-reservadaWidth}|{"Línea",-lineaWidth}|");
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
