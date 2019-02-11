# wj.ObjectComparer

## English

[Español](#Español)

With this library any developer can easily compare objects of the same class or objects of entirely different classes on a property-by-property basis.

The most basic functionality is based on property names:  Properties in the different objects are matched to one another if their name matches.  Name matching is case sensitive.  However, this library allows custom property mapping either via the `PropertyMapAttribute` attribute or by using fluent configuration syntax.

There are two major areas where such a comparer is useful:  Unit testing of object mappers, and applications that rely heavily on data modeling.  It can be used to quickly determine changes between versions of a data record, or taking decisions about the differences in data between a model and a corresponding ViewModel, for instance.

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

So I may have lied.  This is not *really* necessary.  If you use fluent configuration syntax, the type is automatically scanned.  However, the current implementation will not cache the result, so every time a new comparer configuration object is created via the `ComparerConfigurator.Configure()` method, the type will be scanned.  It will be an unnecessary performance hit.  So it is always best to scan a type as shown above because while fluent configuration does not update the scanner's cache, it does consult this cache.

If you have access to the source code of the data types that will be compared, you may use the `PropertyMapAttribute` and `IgnoreForComparisonAttribute` attributes to fine tune object comparison.  The former can be used to map a property to another of an arbitrary name and type in a target type, and you can add as many of these attributes as target types you have; or you can use it to ignore a property when comparing against a target type.  The latter attribute is only for configuring how to ignore a property and it is used if no specific target type comes to mind, or if for some reason the target type is the data type that contains the property (same data type object comparison).

**NOTE**:  If you do not have access to the source code of the data types that will be compared, see [fluent syntax configuration](#fluent-syntax-configuration) below, but make sure you understand the attributes explained here, as their explanation is pretty much the same for the fluent syntax counterparts.

```c#
public class MyModel
{
    //Map MyModel.Id to MyModelVM.ModelId.
    [PropertyMap(typeof(MyModelVM, PropertyMapOptions.MapToProperty, nameof(MyModelVM.ModelId)))]
    public long Id { get; set; }

    //Ignore property RowVersion when comparing against MyModelVM.
    [PropertyMap(typeof(MyModelVM, PropertyMapOptions.IgnoreProperty))]
    public byte[] RowVersion { get; set; }

    //Ignore ModifiedBy for comparisons against any other data type.
    [IgnoreForComparison]
    public string ModifiedBy { get; set; }

    //Ignore CreatedBy for comparisons against this data type (MyModel).
    [IgnoreForComparison(IgnorePropertyOptions.IgnoreForSelf)]
    public string CreatedBy { get; set; }

    //Ignore ModifiedOn for all comparisons.
    [IgnoreForComparison(IgnorePropertyOptions.IgnoreForAll)]
    public DateTime? ModifiedOn { get; set; }
}
```

**NOTE:**  Property maps are *not* bidirectional.  Just because in the example above a map exists like so MyModel.Id -> MyModelVM.ModelId, the reverse is not true.  There is no automatic mapping the other way around (MyModelVM.ModelId -> MyModel.Id).

If you have many data types to scan, it could be better to mark them with the `ScanForPropertyComparisonAttribute` attribute and tell the scanner to scan the assembly(ies) that contains the data types instead of registering the types one by one.

```c#
[ScanForPropertyComparison]
public class MyModel { ... }

[ScanForPropertyComparison]
public class MyModelVM { ... }

//Etc.  Any and all data types to be scanned.  Structs are allowed as well.
```

This is how you scan an entire assembly:

```c#
public void SomeOnStartOrMainOrSomeOtherAppropriatePlace()
{
    Scanner.ScanAssembly(Assembly.GetExecutingAssembly());
}
```

#### Step 2:  Creating an Object Comparer

Once the data types involved have been scanned, object comparers that compare objects of those types can be created now.

There are three ways to create object comparers.  The first one is by using the `ObjectComparer`'s constructor.

```c#
//An object comparer to compare objects of different types.
ObjectComparer oc = new ObjectComparer(typeof(MyModel), typeof(MyModelVM));
//An object comparer to compare objects of the same type.
ObjectComparer oc = new ObjectComparer(typeof(MyModel), typeof(MyModel));
```

While rather simple, the syntax for same-type object comparison looks odd and rather ugly, and even the different-type syntax feels a bit cumbersome.  Therefore, the above is usually *never* the preferred way, which brings us to the second way to create the comparers:

```c#
//An object comparer to compare objects of different types.
ObjectComparer oc = ObjectComparer.Create<MyModel, MyModelVM>();
//An object comparer to compare objects of the same type.
ObjectComparer oc = ObjectComparer.Create<MyModel>();
```

The third way is via [fluent syntax configuration](#fluent-syntax-configuration).

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

### Fluent Syntax Configuration

The quickstart explains a simple, flexible and performant way to configure and use object comparer objects.  However, the customization process relies on attributes and the ability to add them to a type.  If the type's source code cannot be modified, this library's usefulness is diminished.

Fluent Syntax Configuration comes to resolve this.  It is a configuration object that can accummulate the same information provided by the the `PropertyMapAttribute` and `IgnoreForComparisonAttribute` attributes and pass it to an object comparer object.  This is the third way of creating an object comparer object:  Create a configuration object, configure it, and then ask it for a new object comparer object.

For simplicity, data types are not required to be pre-registered, but if they are, your application will perform better.

So, step 1 is to create the configuration object.  Do this using the helper class `ComparerConfigurator`.

```c#
//Same type comparison configuration.
var config = ComparerConfigurator.Configure<MyModel>();
//Different type comparison configuration.
var config = ComparerConfigurator.Configure<MyModel, MyModelVM>();
```

Now the `config` variable can be used to map or ignore properties, and eventually create the object comparer object.

```c#
config
    //Map a property to another property.
    .MapProperty(src => src.Id, dst => dst.ModelId)
    //Ignore a property.
    .IgnoreProperty(src => src.RowVersion);

//Create the comparer.
ObjectComparer oc = config.CreateComparer();
```

Since a configuration object is bound since step 1 to specific object data types, it is not possible to set up property ignore configurations that go beyond the target data type.  In other words, there is no such thing as an `IgnoreProperty()` method that allows ignoring for all data types, for example.

The last feature of the configuration object is the ability to add custom implementations of `IComparer`.  This is covered in the next section.

### Custom IComparer Implementations

This library contains a class named `Scanner`.  This is the thread-safe source of type information for all object comparers.  This information is type information that lists all properties and whether to map or ignore specific properties.  But it also has a list of custom `ICommparer` implementations that will be used during property value comparison.  This is the innermost point of customization in the property-by-property object comparison routine.

Since this is the first time this is mentioned in this document, you are probably unaware that comparison is not limited to determine equality:  It actually determines inequality (less than, equal, or greater than) whenever possible.  Date, numeric and string properties return inequality comparison results.  But other types may also yield such details if a suitable `IComparer` object is provided.  How?

There are three ways to provide such an object:

1. Register it with the scanner.
2. Add it to the comparer configuration object via fluent syntax and the `AddComparer()` method.
3. Directly add it to a comparer object via the `Comparers` dictionary.

Create a custom comparer:

```c#
//Example:  Comparer of DateTime values that ignore times.
public class MyComparer : System.Collections.IComparer
{
    private static DateTime StripTime(DateTime d)
    {
        return new DateTime(d.Year, d.Month, d.Day);
    }

    public int Compare(object x, object y)
    {
        //Compare dates ignoring the time.
        DateTime v1 = StripTime((DateTime)x);
        DateTime v2 = StripTime((DateTime)y);
        return v1.CompareTo(v2);
    }
}
```

Register it with the scanner.  If done this way, it will be used globally for any properties of type `DateTime`.

```c#
Scanner.RegisterGlobalComparerForType(typeof(DateTime), new MyComparer());
```

Or if global use in inappropriate and fluent configuration is being used:

```c#
var config = ComparerConfigurator.Configure<MyModel>()
    .AddComparer<DateTime>(new MyComparer());
ObjectComparer oc = config.CreateComparer();
```

Or if global use is inappropriate but fluent configuration is not being used:

```c#
ObjectComparer oc = ObjectComparer.Create<MyModel>();
oc.Comparers.Add(typeof(DateTime), new MyComparer());
```

For more information and advanced usage, refer to the wiki (coming soon).

## Español

[English](#English)

Con esta biblioteca cualquier desarrollador puede fácilmente comparqar objetos de la misma clase u objetos de clases completamente diferentes propiedad por propiedad.

La funcionalidad más básica se basa en nombres de propiedades:  Propiedades en los objetos son pareadas unas con otras si su nombre es igual.  El mapeo (pareo) es sensible a las mayúsculas.  Sin embargo esta biblioteca permite el mapeo personalizado a través del atributo `PropertyMapAttribute` o utilizando la sintaxis fluida de configuración.

Existen dos áreas principales donde un comparador así es útil:  Pruebas unitarias de objetos mapeadores, y aplicaciones que utilizan mucho modelos de datos.  Puede usarse para determinar rápidamente cambios entre versiones de un registro, o tomar decisiones acerda de las diferencias en datos entre un modelo y su correspondiente modelo-vista, por ejemplo.

### Cómo Instalar

La forma más simple es vía NuGet.  El paquete está publicado @ [wj.ObjectComparer](https://www.nuget.org/packages/wj.ObjectComparer).  Si no, entonces el código fuente puede compilarse y referenciar el DLL resultante.

### Inicio Rápido

Siga estos simples pasos:

1. Registre (o escanee) los tipos de datos involucrados en la comparación propiedad por propiedad.
2. Cree un objeto comparador para los tipos de objetos a comparar.  El orden es importante así que escoja sabiamente.
3. Usando el comparador de objetos ejecute cualquiera de las sobrecargas del método `Compare()`.
4. Si utiliza una sobrecarga que devuelve una colección, podrá en este punto examinar individualmente los resultados de cada propiedad.

#### Paso 1:  Registrar un Tipo de Datos

En su forma más básica, esta biblioteca requiere que los tipos de datos de los objetos a comparar estén registrados, o escaneados, antes de intentar la comparación.  Si código intenta construir un objeto comparador para un tipo de objeto no escaneado, se arrojará una excepción de tipo `NoTypeInformationException`.  Ejecute este código una vez por ciclo de vida de la aplicación.

```C#
public void AglunOnStartOMainOAlgunOtroLugarAdecuado()
{
    Scanner.RegisterType(typeof(MyModel));
}
```

Ok, puede ser que haya mentido.  Esto no es *realmente* necesario.  Si se usa syntaxis fluida de configuración, el tipo de datos es escaneado automáticamente.  Sin embargo, la implementación actual no guarda el resutlado, así que cada vez que se cree un nuevo objeto de configuración vía el método `ComparerConfigurator.Configure()`, el tipo será escaneado.  Esto será una carga al desempeño innecesaria.  Por lo tanto siempre es mejor escanear un tipo como se muestra arriba porque si bien es cierto que la sintaxis fluida de configuración no actualiza el caché de tipos del escáner, ciertamente sí lo consulta.

Si tiene acceso al código fuente de los tipos de datos a comparar, puede utilizar los atributos `PropertyMapAttribute` y `IgnoreForComparisonAttribute` para refinar la operación de comparación.  El primero puede usarse para mapear una propiedad a otra de nombre arbitrario en otro tipo de datos, y puede agregarse tantos de estos atributos como tipos de datos a comparar tenga; o puede usarlo para ignorar una propiedad cuando ses compara contra un tipo de datos particular.  El segundo atributo solamente se usa para configurar cómo se ignora una propiedad y es usado si no se tiene un tipo de datos de destino específico en mente, o si por alguna razón el tipo de datos de destino es el mismo tipo de datos (comparación de objetos del mismo tipo).

**NOTA**:  Si no tiene acceso al código fuente de los tipos de datos a comparar, vea [sintaxis fluida de configuración](#sintaxis-fluida-de-configuracion) abajo, pero asegúrese de entender los atributos explicados aquí ya que la explicación es básicamente lo mismo para la contraparte en sintaxis fluida de configuración.

```c#
public class MyModel
{
    //Mapeo de MyModel.Id a MyModelVM.ModelId.
    [PropertyMap(typeof(MyModelVM, PropertyMapOptions.MapToProperty, nameof(MyModelVM.ModelId)))]
    public long Id { get; set; }

    //Ignorar propiedad RowVersion cuando se compara contra MyModelVM.
    [PropertyMap(typeof(MyModelVM, PropertyMapOptions.IgnoreProperty))]
    public byte[] RowVersion { get; set; }

    //Ignorar ModifiedBy cuando se compara contra cualquier otro tipo de datos.
    [IgnoreForComparison]
    public string ModifiedBy { get; set; }

    //Ignorar CreatedBy cuando se compara contra este tipo de datos (MyModel).
    [IgnoreForComparison(IgnorePropertyOptions.IgnoreForSelf)]
    public string CreatedBy { get; set; }

    //Ignorar ModifiedOn para todas las comparaciones.
    [IgnoreForComparison(IgnorePropertyOptions.IgnoreForAll)]
    public DateTime? ModifiedOn { get; set; }
}
```

**NOTA**:  Los mapas de propiedades *no* son bidireccionales.  Solamente porque en el ejemplo existe un mapeo de MyModel.Id -> MyModelVM.ModelId, el opuesto no es cierto.  No hay mapeo automático a la inversa (MyModelVM.ModelId -> MyModel.Id).

Si tiene muchos tipos de datos a escanear, podría ser mejor markarles con el atributo `ScanForPropertyComparisonAttribute` y pedirle al escáner que escanee el(los) ensamblado(s) que contiene los tipos de datos en vez de registrar los tipos uno por uno.

```c#
[ScanForPropertyComparison]
public class MyModel { ... }

[ScanForPropertyComparison]
public class MyModelVM { ... }

//Etc.  Todos los tipos de datos a escanear.  También pueden marcarse structs.
```

Así es como se escanea un ensamblado completo:

```c#
public void AglunOnStartOMainOAlgunOtroLugarAdecuado()
{
    Scanner.ScanAssembly(Assembly.GetExecutingAssembly());
}
```
#### Paso 2:  Creando el Objeto Comparador

Una vez que se han escaneado los tipos de datos, ya es posible crear objetos comparadores para estos tipos.

Existen tres maneras de crear objetos comparadores.  La primera es utilizando el constructor de `ObjectComparer`.

```c#
//Un objeto comparador que compara objetos de diferentes tipos.
ObjectComparer oc = new ObjectComparer(typeof(MyModel), typeof(MyModelVM));
//Un objeto comparador que compara objetos del mismo tipo.
ObjectComparer oc = new ObjectComparer(typeof(MyModel), typeof(MyModel));
```

Aunque relativamente sencillo, la sintaxis para comparación del mismo tipo luce extraña y un tanto fea, e inclusive la sintaxis para la comparación de dos tipos luce incómoda.  Por lo tanto, esta forma usualmente *nunca* es la forma preferida, lo que nos lleva a la segunda forma de crear comparadores:

```c#
//Un objeto comparador que compara objetos de diferentes tipos.
ObjectComparer oc = ObjectComparer.Create<MyModel, MyModelVM>();
//Un objeto comparador que compara objetos del mismo tipo.
ObjectComparer oc = ObjectComparer.Create<MyModel>();
```

La tercera forma es vía [sintaxis fluida de configuración](#sintaxis-fluida-de-configuracion).

#### Todos los Pasos:  Poniendo Todo Junto

Asumiento que el registro de los tipos de datos ya se ha realizado, compare de esta manera:

```c#
MyModel m1 = BusinessLayer.GetMyModel(someArgument);
MyModel m2 = BusinessLayer.GetMyModelNew(someOtherArgument);
ObjectComparer oc = ObjectComparer.Create<MyModel>();
//Compare los objetos.  La variable isDifferent brindará el resultado consolidado en un único valor Booleano.
var results = oc.Compare(m1, m2, out bool isDifferent);
//Puede examinar resultados de propiedad específicos.
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
    //Tal vez quiera grabar esto en la bitácora o algo.
}
else if (pcr.Result != ComparisonResult.Equal)
{
    //Cubra problemas potenciales como propiedad no encontrada en el 2do objeto, o una excepción durante la comparación.
    if ((pcr.Result & ComparisonResult.PropertyNotFound) == ComparisonResult.PropertyNotFound)
    {
        //La propiedad no se encontró en objeto 2.  Solamente sucede cuando se comparan objetos de diferente tipo.
    }
    else if ((pcr.Result & ComparisonResult.Exception) == ComparisonResult.Exception)
    {
        //Ocurrió una excepción durante la comparación.
    }
    else if ((pcr.Result & ComparisonResult.NoComparer) == ComparisonResult.NoComparer)
    {
        //El tipo de datos de la propiedad no implementa IComparable.  Considere crear un IComparer
        //para el tipo de datos de esta propiedad y regístrelo con el escáner o el comparador.
    }
}
```

### Sintaxis Fluida de Configuración

El Inicio Rápido explica una manera simple, flexible y eficiente de configurar y usar objetos comparadores.  Sin embargo, el proceso de personalización depende de atributos y la habilidad de agregarlos a un tipo de datos.  Si el código fuente del tipo de dato no puede ser modificado, la utilidad de esta biblioteca se ve disminuido.

Sintaxis Fluida de Configuración resuelve este problema.  Es un objeto de configuración que puede acumular la misma información provista por los atributos `PropertyMapAttribute` y `IgnoreForComparisonAttribute` y pasarla a un objeto comparador.  Esta es la tercera manera de crear un objeto comparador:  Crear un objeto de configuración, configurarlo y luego solicitarle un nuevo objeto comparador.

Por simplicidad, no es requerido que los tipos de datos sean pre-registrados, pero si lo están, su aplicación desempeñará mejor.

Entonces el paso 1 es crear el objeto de configuración.  Esto se hace usando la clase `ComparerConfigurator`.

```c#
//Configuración de un mismo tipo.
var config = ComparerConfigurator.Configure<MyModel>();
//Configuración de diferentes tipos.
var config = ComparerConfigurator.Configure<MyModel, MyModelVM>();
```

Ahora la variable `config` puede usarse para mapear o ignorar propiedades, y eventualmente crear el objeto comparador.

```c#
config
    //Map a property to another property.
    .MapProperty(src => src.Id, dst => dst.ModelId)
    //Ignore a property.
    .IgnoreProperty(src => src.RowVersion);

//Create the comparer.
ObjectComparer oc = config.CreateComparer();
```

Como el objeto de configuración está ligado desde el paso 1 a tipos de objetos específcos, no es posible configurar ignorar una propiedad más allá del tipo de datos de destino.  En otras palabras, no hay un método `IgnoreProperty()` capaz de ignorar para todos los tipos de datos, por ejemplo.

La última función del objeto de configuración es la habilidad de agregar implementaciones personalizadas de `IComparer`.  Esto se cubre en la siguiente sección.

### Implementaciones Personalizadas de IComparer

Esta biblioteca contiene una clase llamada `Scanner`.  Es la fuente sincronizada de información de tipo para todos los objetos comparadores.  Esta información es infomración de tipo que lista todas las propiedades y los mapeos a otras propiedades o si ignorar propiedades.  Pero también tiene una lista de objetos que implementan personalizaciones de `IComparer` que se usarán durante la comparación de los valores de propiedades.  Este es el punto de personalización del proceso de comparación propiedad por propiedad más profundo.

Como esta es la primera vez que esto se menciona en este documento, probablemente no sabe que la comparación no se limita a determinar equidad:  De hecho determina inequidad (menor que, igual, o mayor que) siempre que sea posible.  Propiedades de fecha, numéricas y de texto todas retornan resultados de comparación de inequidad.  Pero otros tipos también pueden retornar dichos detalles si se provee un objeto `IComparer` apropiado.  ¿Cómo?

Existen tres maneras de proveer tal objeto:

1. Registre el objeto con el escáner.
2. Agregue el objeto al objeto de configuración vía sintaxis fluida y el método `AddComparer()`.
3. Agregue directamente el objeto al objeto comparador vía el diccionario `Comparers`.

Cree una comparación personalizada:

```c#
//Ejemplo:  Comparador de valores DateTime que ignora tiempos.
public class MyComparer : System.Collections.IComparer
{
    private static DateTime StripTime(DateTime d)
    {
        return new DateTime(d.Year, d.Month, d.Day);
    }

    public int Compare(object x, object y)
    {
        //Compare fechas ignorando tiempos.
        DateTime v1 = StripTime((DateTime)x);
        DateTime v2 = StripTime((DateTime)y);
        return v1.CompareTo(v2);
    }
}
```

Regístrelo con el escáner.  Si lo hace de esta manera, será utilizado globalmente para cualquier propiedad de tipo `DateTime`.

```c#
Scanner.RegisterGlobalComparerForType(typeof(DateTime), new MyComparer());
```

O si el uso global es inapropiado y está usando sintaxis fluida de configuración:

```c#
var config = ComparerConfigurator.Configure<MyModel>()
    .AddComparer<DateTime>(new MyComparer());
ObjectComparer oc = config.CreateComparer();
```

O si el uso global es inapropiado pero no utiliza sintaxis fluida:

```c#
ObjectComparer oc = ObjectComparer.Create<MyModel>();
oc.Comparers.Add(typeof(DateTime), new MyComparer());
```

Para más información y usos avanzados, refereirse al Wiki (pronto).