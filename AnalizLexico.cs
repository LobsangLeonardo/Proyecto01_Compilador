using System.Text;
//Analizador Lexico que genera la tabla de tokens.
public class AnalizadorLexico
{
    private string _codigoFuente;
    private int _posicion;
    private int _linea;
    private readonly List<string> _palabrasClave = new List<string> { "if", "else", "while", "return", "for" };
    private readonly List<char> _operadores = new List<char> { '+', '-', '*', '/', '=', '!', '<', '>', '&', '|' ,'≠'};
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