# wj.ObjectComparer

## English

[Español](#Español)

With this library any developer can easily compare objects of the same class or objects of entirely different classes on a property-by-property basis.

The most basic functionality is based on property names:  Properties in the different objects are matched to one another if their name matches.  Name matching is case sensitive.  However, this library allows custom property mapping either via the `PropertyMapAttribute` attribute or by using the fluent configuration syntax.

Its most common usage is applications that rely heavily on data modeling.  It can be used to quickly determine changes between versions of a data record, or taking decisions about the differences in data between a model and a corresponding ViewModel, for instance.

### How to Install

The simplest way is via NuGet.  The package is published @ [wj.ObjectComparer](https://www.nuget.org/packages/wj.ObjectComparer).  If not, then the source code can be compiled and the resulting DLL file can be referenced directly.

### Quickstart

Follow these simple steps:

1. Register (or scan) the data types that will be involved in property-by-property comparison.
2. Create an object comparer object for the object types to be compared.  The order is important, so choose wisely.
3. Using the comparer object from #2 above, run any of the `Compare()` methods and capture the result.
4. If you call any of the overloads that return a collection, you can now examine the individual property results.

#### Step 1:  Registering a Data Type

In its most basic form, this library requires that the types of objects to be compared be registered, or scanned, prior to attempting comparison.  If code attempts to construct an object comparer for an object type not yet scanned, a `NoTypeInformationException` exception will be thrown.  Run the following code only once per lifetime cycle.

```c#
public void SomeOnStartOrMainOrSomeOtherAppropriatePlace()
{
    Scanner.RegisterType(typeof(MyModel));
}
```

The above code is simple and keep model code clean.  It is also the recommended option if you do not have access to modify the model's code in order to use attributed scanning.  There is another method:  Fluent configuration does not require pre-registration of a data type, as it will scan the type on demand, but the current implementation does not cache the result, so performance hits will be seen every time a comparer is created via fluent configuration unless the data type is registered with the scanner.

Speaking of attributed scanning, attributed scanning is super simple and allows you to cover multiple models in an assembly with a single line of code.  The only drawback is that it requires access to the source code of the data types being compared.  This may not always be a possibility.

First, apply the `ScanForPropertyComparisonAttribute` attribute to any data types that will be compared.

```c#
[ScanForPropertyComparison]
public class MyModel { ... }

[ScanForPropertyComparison]
public struct MyStruct { ... }
```

At this point, property mapping is possible.  From within a to-be-scanned data type, mark any properties with the `PropertyMapAttribute` attribute.  You may opt to map to another property, or to ignore the property.  You may also ignore a property altogether for comparison against any other data type with the `IgnoreForComparisonAttribute` attribute.

```c#
[ScanForPropertyComparison]
public class MyModel
{
    //This map tells an object comparer that the Id property of MyModel is compared against
    //the ModelId of the MyModelVM class, of course, when compared against an object of said class!
    [PropertyMap(typeof(MyModelVM), PropertyMapOperation.MapToProperty, nameof(MyModelVM.ModelId))]
    public long Id { get; set; }
    
    //Multiple maps are allowed.  Add one per target data type.
    [PropertyMap(typeof(MyModelVM), PropertyMapOperation.IgnoreProperty)]
    [PropertyMap(typeof(MyModel), PropertyMapOperation.IgnoreProperty)]
    public byte[] RowVersion { get; set; }
    
    //A property that is ignored when comparing against all data types.
    [IgnoreForComparison]
    public string LastModifiedBy { get; set; }
    
    //A property that is ignored when comparing against all data types except for the ones where a map exists.
    //VERSION 0.4.0:  CUMBERSOME FOR SAME-DATA-TYPE COMPARES.  WILL BE IMPROVED IN FUTURE VERSIONS!!
    [IgnoreForComparison]
    [PropertyMap(typeof(MyModel), PropertyMapOperation.MapToProperty, nameof(MyModel.CreatedBy)]
    public string CreatedBy { get; set; }
    ...
}

[ScanForPropertyComparison]
public class MyModelVM { ... }

[ScanForPropertyComparison]
public struct MyStruct { ... }
```

**NOTE:**  Property maps are *not* bidirectional.  Just because in the example above a map exists like so MyModel.Id -> MyModelVM.ModelId, the reverse is not true.  There is no automatic mapping the other way around (MyModelVM.ModelId -> MyModel.Id).

Now request the scanner to scan the assembly containing the models.  Note that the scan can be repeated on other assemblies as well to include all the necessary data types.

```c#
public void SomeOnStartOrMainOrSomeOtherAppropriatePlace()
{
    Scanner.ScanAssembly(Assembly.GetExecutingAssembly());
}
```

#### Step 2:  Creating an Object Comparer

Once the data types involved have been scanned, object comparers that compare objects of those types can be created now.

There are three ways to create object comparers, and this quickstart guide shows two of them.  The first one is by using the `ObjectComparer`'s constructor.

```c#
//An object comparer to compare objects of different types.
ObjectComparer oc = new ObjectComparer(typeof(MyModel), typeof(MyModelVM));
//An object comparer to compare objects of the same type.
ObjectComparer oc = new ObjectComparer(typeof(MyModel), typeof(MyModel));
```

While rather simple, the syntax for same-type object comparison looks odd and rather ugly, and even the different-type syntax feels a bit cumbersome.  Therefore, the above is usually *never* the preferred way, which brings us to the second way to create the comparers.

```c#
//An object comparer to compare objects of different types.
ObjectComparer oc = ObjectComparer.Create<MyModel, MyModelVM>();
//An object comparer to compare objects of the same type.
ObjectComparer oc = ObjectComparer.Create<MyModel>();
```

#### All Steps:  Putting Things Together

Assuming type registration already took place, compare like this:

```c#
MyModel m1 = BusinessLayer.GetMyModel(someArgument);
MyModel m2 = BusinessLayer.GetMyModelNew(someOtherArgument);
ObjectComparer oc = ObjectComparer.Create<MyModel>();
//Compare the objects.  The isDifferent variable will grant the overall result in a single Boolean.
var results = oc.Compare(m1, m2, out bool isDifferent);
//Specific property results can now be examined.
PropertyComparisonResult pcr = results[nameof(MyModel.Total)]; //Results for the "Total" property.
if (pcr.Result == ComparisonResult.LessThan)
{
    //Maybe show the data in a green background.
}
else if (pcr.Result == ComparisonResult.GreaterThan)
{
    //Maybe show the data in a red background.
}
else if (pcr.Result == ComparisonResult.PropertyIgnored)
{
    //Maybe you would like to log this or something.
}
else if (pcr.Result != ComparisonResult.Equal)
{
    //Cover potential problems like property not found in object 2, or an exception raised during comparison.
    if ((pcr.Result & ComparisonResult.PropertyNotFound) == ComparisonResult.PropertyNotFound)
    {
        //Property was not found in object 2.  Only happens when comparing objects of different types.
    }
    else if ((pcr.Result & ComparisonResult.Exception) == ComparisonResult.Exception)
    {
        //An exception occurred during value comparison.
    }
    else if ((pcr.Result & ComparisonResult.NoComparer) == ComparisonResult.NoComparer)
    {
        //The property data type has no IComparable implementation.  Consider creating an IComparer
        //for this property's data type and register it either with the scanner or with the comparer
        //object itself.
    }
}
```

For more information and advanced usage, refer to the wiki (coming soon).

## Español

[English](#English)

Con esta biblioteca cualquier desarrollador puede fácilmente comparar objetos de la misma clase u objetos de clases completamente diferentes propiedad por propiedad.

En su forma más básica, su funcionalidad se basa en nombres de propiedades:  Propiedades en los objetos son pareados si tienen el mismo nombre.  El procedimiento es sensible a las mayúsculas.  Sin embargo, esta biblioteca permite mapeo (pareo) personalizado ya sea via el atributo `PropertyMapAttribute` o bien usando sintaxis fluida de configuración.

El uso más comun está en aplicaciones que se basan fuertemente en modelos de datos.  Puede ser usado para determinar rápidamente cambios entre versiones de un registro de datos, o tomar decisiones acerca de las diferencias en datos entre un modelo y su correspondiente modelo-vista, por ejemplo.

### Cómo Instalar

Lo más simple es via NuGet.  El paquete está publicado @ [wj.ObjectComparer](https://www.nuget.org/packages/wj.ObjectComparer).  Si no, entonces el código fuente puede compilarse y el archivo DLL resultante puede referenciarse directamente.

### Inicio Rápido

Siga estos simples pasos:

1. Registre (o escanee) los tipos de datos que serán expuestos a la comparación propiedad por propiedad.
2. Cree un objeto comparador para los tipos de objeto que se van a comparar.  El orden es importante, así que escoja sabiamente.
3. Usando el comparador creado en el paso #2, ejecute cualquiera de los métodos `Compare()` y capture el resultado.
4. Si utilizó una de las sobrecargas que devuelven una colección, ahora puede examinar los resultados individuales de las propiedades.

#### Paso 1:  Registrando un Tipo de Datos

En su forma más básica, esta biblioteca requiere que los tipos de objetos a comparar estén registrados, o escaneados, antes de intentar cualquier comparación.  Si código intenta construir un objeto comparador para un tipo de objeto no escaneado, se arrojará una excepción de tipo `NoTypeInformationException`.  Ejecute el siguiente código solamente una vez durante el ciclo de vida de la aplicación.

```c#
public void AlgunOnStartOMainOAlgunOtroLugarApropiado()
{
    Scanner.RegisterType(typeof(MyModel));
}
```

El código mostrado arriba es simple y mantiene el código del modelo limpio.  También es la opción recomendada si no se tiene acceso a modificar el código del modelo para utilizar escaneo por atributos.  Existe otro método:  Configuración fluida no requiere de un pre-registro de un tipo de datos ya que lo escaneará cuando se demande, pero la implementación actual no guarda el resultado, así que puede experimentarse detrimentos en desempeño cada vez que se crea un comparador a través de configuración fluida, a menos claro que el tipo de datos esté registrado con el escáner.

Hablando de escaneo por atributos, escaneo por atributos es súper simple y permite cubrir múltiples modelos en un ensamblado con una única línea de código.  El único detalle es que requiere de acceso al código fuente de los tipos de datos que serán comparados.  Esto no siempre es una posibilidad.

Primero aplique el atributo `ScanForPropertyComparisonAttribute` a los tipos de datos que serán comparados.

```c#
[ScanForPropertyComparison]
public class MyModel { ... }

[ScanForPropertyComparison]
public struct MyStruct { ... }
```
En este punto es posible hacer mapeo (o pareo) personalizado de propiedades.  Dentro de una clase que será escaneada, marque cualquier propiedad con el atributo `PropertyMapAttribute`.  Puede optar por parear la propiedad con otra, o bien ignorar la propiedad.  También puede ignorar una propiedad completamente para comparación contra cualquier otro tipo de dato con el atributo `IgnoreForComparisonAttribute`.

```c#
[ScanForPropertyComparison]
public class MyModel
{
    //Este mapa le dice a un comparador que la propiedad Id de MyModel es comparada contra la propiedad
    //ModelId de la clase MyModelVM, obviamente, cuando se compara contra un objeto de dicha clase!
    [PropertyMap(typeof(MyModelVM), PropertyMapOperation.MapToProperty, nameof(MyModelVM.ModelId))]
    public long Id { get; set; }
    
    //Se permiten múltiples mapas.  Agregue uno para cada tipo de datos destino.
    [PropertyMap(typeof(MyModelVM), PropertyMapOperation.IgnoreProperty)]
    [PropertyMap(typeof(MyModel), PropertyMapOperation.IgnoreProperty)]
    public byte[] RowVersion { get; set; }
    
    //Una propiedad ignorada para comparación contra cualquier tipo de datos.
    [IgnoreForComparison]
    public string LastModifiedBy { get; set; }
    
    //Una propiedad ignorada para comparación contra cualquier tipo de datos excepto aquellos que tienen mapa.
    //VERSIÓN 0.4.0:  INCÓMODO PARA COMPARACIONES CON EL MISMO TIPO.  SE MEJORARÁ EN FUTURAS VERSIONES!!
    [IgnoreForComparison]
    [PropertyMap(typeof(MyModel), PropertyMapOperation.MapToProperty, nameof(MyModel.CreatedBy)]
    public string CreatedBy { get; set; }
    ...
}

[ScanForPropertyComparison]
public class MyModelVM { ... }

[ScanForPropertyComparison]
public struct MyStruct { ... }
```

**NOTA:**  Los mapas de propiedades *no* son bidireccionales.  Solamente porque en el ejemplo mostrado existe un mapa MyModel.Id -> MyModelVM.ModelId, el inverso no es cierto.  No hay un mapeo automático para la forma inversa (MyModelVM.ModelId -> MyModel.Id).

Ahora solicite al escáner que escanee el ensamblado que contiene los modelos.  Nótese que este escaneo puede repetirse para otros ensamblados de igual manera para incluir todos los tipos de datos necesarios.

```c#
public void AlgunOnStartOMainOAlgunOtroLugarApropiado()
{
    Scanner.ScanAssembly(Assembly.GetExecutingAssembly());
}
```

#### Paso 2:  Creando el Objeto Comparador

Una vez que los tipos de datos involucrados han sido escaneados, se puede ahora crear objetos comparadores que comparan objetos de esos tipos.

Existen tres maneras de crear objetos comparadores, y esta guía rápida muestra dos de ellas.  La primera es utilizando el constructor de la clase `ObjectComparer`.

```c#
//Un objeto comparador para comparar objetos de tipos diferentes.
ObjectComparer oc = new ObjectComparer(typeof(MyModel), typeof(MyModelVM));
//Un objeto comparador para comparar objetos del mismo tipo.
ObjectComparer oc = new ObjectComparer(typeof(MyModel), typeof(MyModel));
```

Aunque relativamente sencillo, la sintaxis para el objeto que compara objetos del mismo tipo luce extraña y algo fea, e inclusive la sintaxis para el que compara tipos diferentes se siente un poco incómoda.  Por lo tanto, este método usualmente *nunca* es la forma preferida, lo que nos lleva a la segunda manera de crear comparadores.

```c#
//Un objeto comparador para comparar objetos de tipos diferentes.
ObjectComparer oc = ObjectComparer.Create<MyModel, MyModelVM>();
//Un objeto comparador para comparar objetos del mismo tipo.
ObjectComparer oc = ObjectComparer.Create<MyModel>();
```

#### Todos los Pasos:  Poniendo Todo Junto

Asumiendo que el registro de tipos ya ha sido ejecutado, compare objetos de esta manera:

```c#
MyModel m1 = BusinessLayer.GetMyModel(someArgument);
MyModel m2 = BusinessLayer.GetMyModelNew(someOtherArgument);
ObjectComparer oc = ObjectComparer.Create<MyModel>();
//Compare los objetos.  La variable isDifferent brindará el resultado general en un único Booleano.
var results = oc.Compare(m1, m2, out bool isDifferent);
//Ahora pueden examinarse los resultados de comparación de propiedades.
PropertyComparisonResult pcr = results[nameof(MyModel.Total)]; //Resultados para la propiedad "Total".
if (pcr.Result == ComparisonResult.LessThan)
{
    //Tal vez mostrar el dato en un fondo verde.
}
else if (pcr.Result == ComparisonResult.GreaterThan)
{
    //Tal vez mostrar el dato en un fondo rojo.
}
else if (pcr.Result == ComparisonResult.PropertyIgnored)
{
    //Tal vez registrar esto en una bitácora o algo similar.
}
else if (pcr.Result != ComparisonResult.Equal)
{
    //Cubrir problemas potenciales como propiedad no encontrada en objeto 2, o una excepción durante la comparación.
    if ((pcr.Result & ComparisonResult.PropertyNotFound) == ComparisonResult.PropertyNotFound)
    {
        //Propiedad no encontrada en el objeto 2.  Sólo pasa cuando se comparan objetos de diferente tipo.
    }
    else if ((pcr.Result & ComparisonResult.Exception) == ComparisonResult.Exception)
    {
        //Ocurrió una excepción durante la comparación del valor.
    }
    else if ((pcr.Result & ComparisonResult.NoComparer) == ComparisonResult.NoComparer)
    {
        //El tipo de dato de la propiedad no implementa IComparable.  Considere crear un IComparer
        //para este tipo de datos de propiedad y registrarlo ya sea con el escáner o con el mismo
        //objeto comparador.
    }
}
```

Para más información y usos avanzados, refiérase al wiki (pronto).
