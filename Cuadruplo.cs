using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

public class GeneradorCuadruplos
{
    private List<Cuadruplo> cuadruplos;
    private int contadorTemporal;

    public GeneradorCuadruplos()
    {
        cuadruplos = new List<Cuadruplo>();
        contadorTemporal = 1;
    }

    public List<Cuadruplo> GenerarCuadruplos(NodoAST raiz)
    {
        cuadruplos.Clear();
        contadorTemporal = 1;
        RecorrerAST(raiz);
        return cuadruplos;
    }

    private void RecorrerAST(NodoAST nodo)
    {
        if (nodo == null) return;

        string valorNodo = !string.IsNullOrEmpty(nodo.Valor) ? nodo.Valor.ToLower() : 
                           !string.IsNullOrEmpty(nodo.Tipo) ? nodo.Tipo.ToLower() : "";

        switch (valorNodo)
        {
            case "if":
                GenerarCuadruploIf(nodo);
                break;
            case "programa":
            case "bloque":
                foreach (var hijo in nodo.Hijos)
                {
                    RecorrerAST(hijo);
                }
                break;
            case "while":
                GenerarCuadruploWhile(nodo);
                break;
            case "for":
                GenerarCuadruploFor(nodo);
                break;
            case "return":
                GenerarCuadruploReturn(nodo);
                break;
            case "=":
                GenerarCuadruploAsignacion(nodo);
                break;
            default:
                if (nodo.Hijos.Count > 0)
                {
                    foreach (var hijo in nodo.Hijos)
                    {
                        RecorrerAST(hijo);
                    }
                }
                break;
        }
    }

    private void GenerarCuadruploAsignacion(NodoAST nodo)
    {
        if (nodo.Hijos.Count != 2) return;

        string variable = nodo.Hijos[0].Valor;
        string valorOTemporal = ProcesarExpresion(nodo.Hijos[1]);

        cuadruplos.Add(new Cuadruplo("=", valorOTemporal, "_", variable));
    }

    private void GenerarCuadruploIf(NodoAST nodo)
    {
        if (nodo.Hijos.Count < 2) return;

        string condicion = ProcesarExpresion(nodo.Hijos[0]);
        string etiquetaFalso = GenerarEtiqueta();
        string etiquetaSalida = GenerarEtiqueta();

        cuadruplos.Add(new Cuadruplo("IF_FALSE", condicion, etiquetaFalso, "_"));

        RecorrerAST(nodo.Hijos[1]);

        if (nodo.Hijos.Count == 3)
        {
            cuadruplos.Add(new Cuadruplo("GOTO", etiquetaSalida, "_", "_"));
            cuadruplos.Add(new Cuadruplo("LABEL", etiquetaFalso, "_", "_"));
            RecorrerAST(nodo.Hijos[2]);
            cuadruplos.Add(new Cuadruplo("LABEL", etiquetaSalida, "_", "_"));
        }
        else
        {
            cuadruplos.Add(new Cuadruplo("LABEL", etiquetaFalso, "_", "_"));
        }
    }

    private void GenerarCuadruploWhile(NodoAST nodo)
    {
        if (nodo.Hijos.Count < 2) return;

        string etiquetaInicio = GenerarEtiqueta();
        string etiquetaSalida = GenerarEtiqueta();

        cuadruplos.Add(new Cuadruplo("LABEL", etiquetaInicio, "_", "_"));

        string condicion = ProcesarExpresion(nodo.Hijos[0]);
        cuadruplos.Add(new Cuadruplo("IF_FALSE", condicion, etiquetaSalida, "_"));

        RecorrerAST(nodo.Hijos[1]);
        cuadruplos.Add(new Cuadruplo("GOTO", etiquetaInicio, "_", "_"));
        cuadruplos.Add(new Cuadruplo("LABEL", etiquetaSalida, "_", "_"));
    }

    private void GenerarCuadruploFor(NodoAST nodo)
    {
        if (nodo.Hijos.Count < 4) return;

        RecorrerAST(nodo.Hijos[0]);

        string etiquetaInicio = GenerarEtiqueta();
        string etiquetaSalida = GenerarEtiqueta();

        cuadruplos.Add(new Cuadruplo("LABEL", etiquetaInicio, "_", "_"));

        string condicion = ProcesarExpresion(nodo.Hijos[1]);
        cuadruplos.Add(new Cuadruplo("IF_FALSE", condicion, etiquetaSalida, "_"));

        RecorrerAST(nodo.Hijos[3]);

        RecorrerAST(nodo.Hijos[2]);

        cuadruplos.Add(new Cuadruplo("GOTO", etiquetaInicio, "_", "_"));
        cuadruplos.Add(new Cuadruplo("LABEL", etiquetaSalida, "_", "_"));
    }

    private void GenerarCuadruploReturn(NodoAST nodo)
    {
        if (nodo.Hijos.Count > 0)
        {
            string valorRetorno = ProcesarExpresion(nodo.Hijos[0]);
            cuadruplos.Add(new Cuadruplo("RETURN", valorRetorno, "_", "_"));
        }
        else
        {
            cuadruplos.Add(new Cuadruplo("RETURN", "_", "_", "_"));
        }
    }

    private string ProcesarExpresion(NodoAST nodo)
    {
        if (nodo == null) return "_";

        // Si es un nodo hoja (constante o variable)
        if (nodo.Hijos.Count == 0)
        {
            // Prioriza el valor, luego el tipo
            return !string.IsNullOrEmpty(nodo.Valor) ? nodo.Valor : 
                   !string.IsNullOrEmpty(nodo.Tipo) ? nodo.Tipo : "_";
        }

        // Para operaciones binarias (+, -, *, /, comparaciones)
        if (nodo.Hijos.Count == 2)
        {
            string operando1 = ProcesarExpresion(nodo.Hijos[0]);
            string operando2 = ProcesarExpresion(nodo.Hijos[1]);
            string temporal = GenerarTemporal();

            // Usar el valor de la operación del nodo actual
            string operacion = !string.IsNullOrEmpty(nodo.Valor) ? nodo.Valor : 
                               !string.IsNullOrEmpty(nodo.Tipo) ? nodo.Tipo : "+";

            cuadruplos.Add(new Cuadruplo(operacion, operando1, operando2, temporal));
            return temporal;
        }

        // Para operaciones unarias o expresiones con un solo hijo
        if (nodo.Hijos.Count == 1)
        {
            return ProcesarExpresion(nodo.Hijos[0]);
        }

        return "_";
    }

    private string GenerarTemporal()
    {
        return $"t{contadorTemporal++}";
    }

    private string GenerarEtiqueta()
    {
        return $"L{contadorTemporal++}";
    }
    public string GenerarTabla()
    {
        // Definir el ancho de las columnas
        int anchoNumero = 5;
        int anchoOperacion = 15;
        int anchoOperando = 15;
        int anchoResultado = 15;

        // Crear un StringBuilder para construir la tabla
        StringBuilder tabla = new StringBuilder();

        // Encabezados
        tabla.AppendLine(
            "+" + new string('-', anchoNumero) + 
            "+" + new string('-', anchoOperacion) + 
            "+" + new string('-', anchoOperando) + 
            "+" + new string('-', anchoOperando) + 
            "+" + new string('-', anchoResultado) + "+"
        );

        tabla.AppendLine(
            "| " + "No.".PadRight(anchoNumero - 1) +
            "| " + "Operación".PadRight(anchoOperacion - 1) +
            "| " + "Operando 1".PadRight(anchoOperando - 1) +
            "| " + "Operando 2".PadRight(anchoOperando - 1) +
            "| " + "Resultado".PadRight(anchoResultado - 1) + "|"
        );

        // Línea separadora
        tabla.AppendLine(
            "+" + new string('-', anchoNumero) + 
            "+" + new string('-', anchoOperacion) + 
            "+" + new string('-', anchoOperando) + 
            "+" + new string('-', anchoOperando) + 
            "+" + new string('-', anchoResultado) + "+"
        );

        // Agregar cada cuádruplo
        for (int i = 0; i < cuadruplos.Count; i++)
        {
            tabla.AppendLine(
                "| " + i.ToString().PadRight(anchoNumero - 1) +
                "| " + cuadruplos[i].Operacion.PadRight(anchoOperacion - 1) +
                "| " + cuadruplos[i].Operando1.PadRight(anchoOperando - 1) +
                "| " + cuadruplos[i].Operando2.PadRight(anchoOperando - 1) +
                "| " + cuadruplos[i].Resultado.PadRight(anchoResultado - 1) + "|"
            );
        }

        // Línea final
        tabla.AppendLine(
            "+" + new string('-', anchoNumero) + 
            "+" + new string('-', anchoOperacion) + 
            "+" + new string('-', anchoOperando) + 
            "+" + new string('-', anchoOperando) + 
            "+" + new string('-', anchoResultado) + "+"
        );

        return tabla.ToString();
    }

    public void ImprimirCuadruplos()
    {
        Console.WriteLine(GenerarTabla());
    }

    public void ExportarCuadruplosAArchivo(string nombreArchivo)
    {
        File.WriteAllText(nombreArchivo, GenerarTabla());
    }
}

public class Cuadruplo
{
    public string Operacion { get; }
    public string Operando1 { get; }
    public string Operando2 { get; }
    public string Resultado { get; }

    public Cuadruplo(string operacion, string operando1, string operando2, string resultado)
    {
        Operacion = operacion;
        Operando1 = operando1;
        Operando2 = operando2;
        Resultado = resultado;
    }

    public override string ToString()
    {
        return $"({Operacion}, {Operando1}, {Operando2}, {Resultado})";
    }
}