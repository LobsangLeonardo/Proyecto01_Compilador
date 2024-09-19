using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

//---------------------------Ideas para agregar -----------------------------------------
// Ejecucion en una tabla, si es reservada o no
// Poner la selecion del archivo en la ejecucion en consola
// Si la palabra es reservada
// Ignorar loa comentarios
// Numero de linea
// PONER COMENTARIOS 

public enum TipoToken
{
    PalabraClave,
    Identificador,
    Operador,
    Numero,
    Delimitador,
    Comentario,
    Cadena,
    Error,
    Desconocido
}

public class Token
{
    public TipoToken Tipo { get; set; }
    public string Valor { get; set; }

    public Token(TipoToken tipo, string valor)
    {
        Tipo = tipo;
        Valor = valor;
    }

    public override string ToString()
    {
        return $"[{Tipo}] {Valor}";
    }
}

public class AnalizadorLexico
{
    private string _codigoFuente;
    private int _posicion;
    private readonly List<string> _palabrasClave = new List<string> { "if", "else", "while", "return" };
    private readonly List<char> _operadores = new List<char> { '+', '-', '*', '/', '=', '!', '<', '>', '&', '|' };
    private readonly List<char> _delimitadores = new List<char> { '(', ')', '{', '}', ';', ',' };

    public AnalizadorLexico(string codigoFuente)
    {
        _codigoFuente = codigoFuente;
        _posicion = 0;
    }

    public List<Token> Analizar()
    {
        List<Token> tokens = new List<Token>();

        while (_posicion < _codigoFuente.Length)
        {
            char caracterActual = _codigoFuente[_posicion];

            if (char.IsWhiteSpace(caracterActual))
            {
                _posicion++;
                continue;
            }

            if (char.IsLetter(caracterActual))
            {
                string identificador = LeerMientras(char.IsLetterOrDigit);
                if (_palabrasClave.Contains(identificador))
                {
                    tokens.Add(new Token(TipoToken.PalabraClave, identificador));
                }
                else
                {
                    tokens.Add(new Token(TipoToken.Identificador, identificador));
                }
            }
            else if (char.IsDigit(caracterActual) || caracterActual == '-')
            {
                string numero = LeerNumero();
                tokens.Add(new Token(TipoToken.Numero, numero));
            }
            else if (caracterActual == '"')
            {
                _posicion++; 
                string cadena = LeerMientras(c => c != '"');

                if (_posicion < _codigoFuente.Length && _codigoFuente[_posicion] == '"')
                {
                    _posicion++; 
                    tokens.Add(new Token(TipoToken.Cadena, cadena));
                }
                else
                {
                    tokens.Add(new Token(TipoToken.Error, $"Cadena sin cerrar: \"{cadena}"));
                }
            }
            else if (_operadores.Contains(caracterActual))
            {
                if (caracterActual == '/' && VerProximo() == '/')
                {
                    _posicion += 2;
                    string comentario = LeerMientras(c => c != '\n');
                    tokens.Add(new Token(TipoToken.Comentario, "//" + comentario));
                }
                else if (caracterActual == '/' && VerProximo() == '*')
                {
                    _posicion += 2;
                    string comentario = LeerHasta("*/");
                    tokens.Add(new Token(TipoToken.Comentario, "/*" + comentario + "*/"));
                }
                else
                {
                    string operador = LeerMientras(c => _operadores.Contains(c));
                    tokens.Add(new Token(TipoToken.Operador, operador));
                }
            }
            else if (_delimitadores.Contains(caracterActual))
            {
                tokens.Add(new Token(TipoToken.Delimitador, caracterActual.ToString()));
                _posicion++;
            }
            else
            {
                tokens.Add(new Token(TipoToken.Error, $"Carácter no reconocido: {caracterActual}"));
                _posicion++;
            }
        }

        return tokens;
    }

    private char VerProximo()
    {
        return _posicion + 1 < _codigoFuente.Length ? _codigoFuente[_posicion + 1] : '\0';
    }

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

class Programa_Impri
{
    static void Main(string[] args)
    {
        Console.WriteLine("Coloque el nombre del archivo con su tipo:");
        string rutaArchivo = Console.ReadLine();

        if (!File.Exists(rutaArchivo))
        {
            Console.WriteLine("Error, no se encuentra un archivo con ese nombre.");
            return;
        }

        string codigoFuente = File.ReadAllText(rutaArchivo);

        AnalizadorLexico analizador = new AnalizadorLexico(codigoFuente);
        List<Token> tokens = analizador.Analizar();

        
        const int tipoWidth = 15;
        const int valorWidth = 60;
        const int reservadaWidth = 10;

        
        Console.WriteLine(new string('-', tipoWidth + valorWidth + reservadaWidth + 4));
        Console.WriteLine($"|{"Tipo",-tipoWidth}|{"Valor",-valorWidth}|{"Reservada",-reservadaWidth}|");
        Console.WriteLine(new string('-', tipoWidth + valorWidth + reservadaWidth + 4));


        foreach (var token in tokens)
        {
            string reservada = token.Tipo == TipoToken.PalabraClave ? "Sí" : "No";
            Console.WriteLine($"|{token.Tipo,-tipoWidth}|{token.Valor,-valorWidth}|{reservada,-reservadaWidth}|");
        }

        Console.WriteLine(new string('-', tipoWidth + valorWidth + reservadaWidth + 4));
    }
}
