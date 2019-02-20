![WebJose's ObjectComparer](Logo_128.png)
# wj.ObjectComparer

## English

[Español](#Español)

With this library any developer can easily compare objects of the same class or objects of entirely different classes on a property-by-property basis.

There are two major areas where such a comparer is useful:  Unit testing of object mappers, and applications that rely heavily on data modeling.  It can be used to quickly determine changes between versions of a data record, or taking decisions about the differences in data between a model and a corresponding ViewModel, for instance.

### How It Works

The most basic functionality is based on property names:  Properties in the different objects are matched to one another if their name matches.  Name matching is case sensitive.  However, this library allows custom property mapping either via the `PropertyMapAttribute` attribute or by using **fluent syntax configuration**.

The actual property-by-property comparison is done using implementations of the `IComparer` interface.  Custom comparer objects for specific data types may also be provided, either directly to the `Scanner` class for global use, or directly to an object comparer object for local use.

In the case of .Net Framework v4.6.1 or later, this library supports the use of the different attribute classes applied to the data type in the `MetadataTypeAttribute` attribute.

### How to Install

The simplest way is via NuGet.  The package is published @ [wj.ObjectComparer](https://www.nuget.org/packages/wj.ObjectComparer).  If not, then the source code can be compiled and the resulting DLL file can be referenced directly, or you can download a zip file from the [releases](../../releases) page containing the compiled DLL.

### Quickstart

This library can be used in many situations.  The following subsections will quickstart from the simplest case to the most complex one.

**IMPORTANT**:  This quickstart will try to cover the most common cases, but other cases are possible.  Also make sure you read **Case A** regardless of your particular case as code shown in this case is not repeated in the other cases.

#### Case A:  As-Is Comparison

**Premises:**

1. The objects to be compared are of the same type, or different data types but property names match exactly.
2. The data types are known.

**Steps:**

1. Register (or scan) the data types that will be involved in property-by-property comparison, once per application lifecycle.
2. Create an object comparer object for the object types to be compared.  The order is important, so choose wisely.
3. Using the object comparer object, run any of the `Compare()` methods and capture the result.
4. If you called any of the overloads that return a collection, you can now examine the individual property results.

```c#
//Type registration is only done once per application lifecyle, such as a Main() or OnStart() method.
public static void Main(string[] args)
{
    Scanner.RegisterType<MyModel>();
    Scanner.RegisterType<MyModelVM>();
}

//Now object comparer objects can be created for any combination of the registered data types.
//Compare objects of the same type:
ObjectComparer oc1 = ObjectComparer.Create<MyModel>();
//Compare objects of different types:
ObjectComparer oc2 = ObjectComparer.Create<MyModel, MyModelVM>();
//Etc.  Can be done for MyModelVM -> MyModel, or two MyModelVM's.

//Compare objects.
MyModel m1 = ObtainModelSomehow();
MyModel m2 = ObtainModelSomehow();
MyModelVM vm = CreateVMSomehow();
var resultsMtoM = oc1.Compare(m1, m2, out bool mToMIsDifferent);
var resultsMtoVM = oc2.Compare(m1, vm, out bool mToVMIsDifferent);
//The Boolean out's are meant to give an overall result without having to traverse the collection
//of property results.
Logger.Information($"Model to model comparison says objects are {(mToMIsDifferent ? "different" : "equal")}.");
Logger.Information($"Model to view-model comparison says objects are {(mToVMIsDifferent ? "different" : "equal")}.");
//Or run a loop through the collection of property results, or access an individual result by
//property name.
foreach (PropertyComparisonResult pcr in resultsMtoM)
{
    //The PropertyComparisonResult class has a custom ToString().
    Logger.Information($"{pcr}");
}
//Individual property result examination:
PropertyComparisonResult pcrSomeProperty = resultsMtoM[nameof(MyModel.SomeProperty)];
if (pcrSomeProperty.Result == ComparisonResult.LessThan)
{
    //Maybe show the data in a green background.
}
else if (pcrSomeProperty.Result == ComparisonResult.GreaterThan)
{
    //Maybe show the data in a red background.
}
else if (pcrSomeProperty.Result == ComparisonResult.PropertyIgnored)
{
    //Maybe you would like to log this or something.
}
else if ((pcrSomeProperty.Result & ComparisonResult.Equal) == ComparisonResult.Undefined)
{
    //Cover potential problems like property not found in object 2, or an exception raised during comparison.
    if ((pcrSomeProperty.Result & ComparisonResult.PropertyNotFound) == ComparisonResult.PropertyNotFound)
    {
        //Property was not found in object 2.  Only happens when comparing objects of different types.
    }
    else if ((pcrSomeProperty.Result & ComparisonResult.Exception) == ComparisonResult.Exception)
    {
        //An exception occurred during value comparison.
    }
    else if ((pcrSomeProperty.Result & ComparisonResult.NoComparer) == ComparisonResult.NoComparer)
    {
        //The property data type has no IComparable implementation.  Consider creating an IComparer
        //for this property's data type and register it either with the scanner or with the comparer
        //object itself.
    }
}
```

#### Case B.1:  A Property Map Is Needed

**Premises:**

1. The objects to be compared are of different data types.
2. The data types are known.
3. At least one property from the source has a different name in the target.
4. Property mapping is needed everywhere in the application (globally).
5. The source code of the source data type is available for modification.

**Steps:**

1. Add the `PropertyMapAttribute` attribute to each property in the source data type that has a different name in the target data type.
2. Register (or scan) the data types that will be involved in property-by-property comparison, once per application lifecycle.
3. Create an object comparer object for the object types to be compared.  The order is important, so choose wisely.
4. Using the object comparer object, run any of the `Compare()` methods and capture the result.
5. If you called any of the overloads that return a collection, you can now examine the individual property results.

```c#
//This is how a property map is done when using attributes.
public class MyModel
{
    [PropertyMap(typeof(MyModelVM), PropertyMapOperation.MapToProperty, nameof(MyModelVM.DateOfBirth))]
    public DateTime BirthDate { get; set; }
}
```

**NOTE:**  Property maps are *not* bidirectional.  Just because in the example above a map exists like so MyModel.BirthDate -> MyModelVM.DateOfBirth, the reverse is not true.  There is no automatic mapping the other way around (MyModelVM.DateOfBirth -> MyModel.BirthDate).

#### Case B.2:  A Property Map Is Needed

**Premises:**

1. The objects to be compared are of different data types.
2. The data types are known.
3. At least one property from the source has a different name in the destination.
4. Property mapping is needed everywhere in the application (globally).
5. The source code of the source data type is not available for modification or unwillingness to modify it exists.

**Steps:**

1. Configure the source type once per lifecycle with the needed property map using the `MapProperty()` method.
2. Create an object comparer object for the object types to be compared.  The order is important, so choose wisely.
3. Using the object comparer object, run any of the `Compare()` methods and capture the result.
4. If you called any of the overloads that return a collection, you can now examine the individual property results.

```c#
public static void Main(string[] args)
{
    //The source type may or may not be pre-registered in the scanner.  It does not matter.
    Scanner.ConfigureType<MyModel>().ForType<MyModelVM>()
        .MapProperty(src => src.BirthDate, dst => dst.DateOfBirth);
}
```

#### Case B.3:  A Property Map Is Needed

**Premises:**

1. The objects to be compared are of different data types.
2. The data types are known.
3. At least one property from the source has a different name in the destination.
4. Property mapping is not needed everywhere in the application (locally).

**Steps:**

1. Create a comparer configuration object using the appropriate `ComparerConfigurator.Configure()` method.
2. Using the configuration object, add the needed property map via the `MapProperty()` method.
3. Create an object comparer object for the object types to be compared via the configuration object's `CreateComparer()` method.
4. Using the object comparer object, run any of the `Compare()` methods and capture the result.
5. If you called any of the overloads that return a collection, you can now examine the individual property results.

```c#
ObjectComparer oc = ComparerConfigurator.Configure<MyModel, MyModelVM>()
    .MapProperty(src => src.BirthDate, dst => dst.DateOfBirth)
    .CreateComparer();
```

#### Case C.1:  A Property Must Be Ignored For a Specific Target Type

**Premises:**

1. The objects to be compared are of different data types.
2. The data types are known.
3. At least one property from the source must be ignored when comparing against objects of the target type.
4. Ignoring this property is needed everywhere in the application (globally).
5. The source code of the source data type is available for modification.

**Steps:**

1. Add the `PropertyMapAttribute` attribute to each property in the source data type that must be ignored when comparing against objects of the target data type.
2. Create an object comparer object for the object types to be compared.  The order is important, so choose wisely.
3. Using the object comparer object, run any of the `Compare()` methods and capture the result.
4. If you called any of the overloads that return a collection, you can now examine the individual property results.

```c#
public class MyModel
{
    [PropertyMap(typeof(MyModelVM), PropertyMapOperation.IgnoreProperty)]
    public DateTime BirthDate { get; set; }
}
```

#### Case C.2:  A Property Must Be Ignored For a Specific Target Type

**Premises:**

1. The objects to be compared are of different data types.
2. The data types are known.
3. At least one property from the source must be ignored when comparing against objects of the target type.
4. Ignoring this property is needed everywhere in the application (globally).
5. The source code of the source data type is not available for modification or unwillingness to modify it exists.

**Steps:**

1. Configure the source type once per lifecycle with the needed property map using the `IgnoreProperty()` method.
2. Create an object comparer object for the object types to be compared.  The order is important, so choose wisely.
3. Using the object comparer object, run any of the `Compare()` methods and capture the result.
4. If you called any of the overloads that return a collection, you can now examine the individual property results.

```c#
public static void Main(string[] args)
{
    Scanner.ConfigureType<MyModel>.ForType<MyModelVM>()
        .IgnoreProperty(src => src.BirthDate);
}
```

#### Case C.3:  A Property Must Be Ignored For a Specific Target Type

**Premises:**

1. The objects to be compared are of different data types.
2. The data types are known.
3. At least one property from the source must be ignored when comparing against objects of the target type.
4. Ignoring this property is not needed everywhere in the application (locally).

**Steps:**

1. Create a comparer configuration object using the appropriate `ComparerConfigurator.Configure()` method.
2. Using the configuration object, ignore the property via the `IgnoreProperty()` method.
3. Create an object comparer object for the object types to be compared via the configuration object's `CreateComparer()` method.
4. Using the object comparer object, run any of the `Compare()` methods and capture the result.
5. If you called any of the overloads that return a collection, you can now examine the individual property results.

```c#
ObjectComparer oc = ComparerConfigurator.Configure<MyModel, MyModelVM>()
    .IgnoreProperty(src => src.BirthDate)
    .CreateComparer();
```

#### Case D.1:  A Property Must Be Ignored For Unspecified Target Types

**Premises:**

1. The objects to be compared are of equal or different data types.
2. The target data types are unknown or there is unwilligness to specify them, or the target data type is the same as the source data type.
3. At least one property from the source must be ignored when comparing against objects of the unspecified target types.
4. Ignoring this property is needed everywhere in the application (globally).
5. The source code of the source data type is available for modification.

**Steps:**

1. Add the `IgnoreForComparisonAttribute` attribute to each property in the source data type that must be ignored using the appropriate value.
2. Register (or scan) the data types that will be involved in property-by-property comparison, once per application lifecycle.
3. Create an object comparer object for the object types to be compared.  The order is important, so choose wisely.
4. Using the object comparer object, run any of the `Compare()` methods and capture the result.
5. If you called any of the overloads that return a collection, you can now examine the individual property results.

```c#
public class MyModel
{
    [IgnoreForComparison]
    public DateTime IgnoredForOtherDataTypes { get; set; }

    [IgnoreForComparison(IgnorePropertyOptions.IgnoreForSelf)]
    public DateTime IgnoredForSelfDataType { get; set; }

    [IgnoreForComparison(IgnorePropertyOptions.IgnoreForAll)]
    public DateTime IgnoredForAllDataTypes { get; set; }
}
```

#### Case D.2:  A Property Must Be Ignored For Unspecified Target Types

**Premises:**

1. The objects to be compared are of equal or different data types.
2. The target data types are unknown or there is unwilligness to specify them, or the target data type is the same as the source data type.
3. At least one property from the source must be ignored when comparing against objects of the unspecified target types.
4. Ignoring this property is needed everywhere in the application (globally).
5. The source code of the source data type is not available for modification or unwillingness to modify it exists.

**Steps:**

1. Configure the source type once per lifecycle using the `IgnoreProperty()` method.
2. Create an object comparer object for the object types to be compared.  The order is important, so choose wisely.
3. Using the object comparer object, run any of the `Compare()` methods and capture the result.
4. If you called any of the overloads that return a collection, you can now examine the individual property results.

```c#
public static void Main(string[] args)
{
    Scanner.ConfigureType<MyModel>
        .IgnoreProperty(src => src.BirthDate, IgnorePropertyOptions.IgnoreForAll);
}
```

### More About Attribute-Based Configuration

If you have many data types to scan, it could be better to mark them with the `ScanForPropertyComparisonAttribute` attribute and tell the scanner to scan the assembly(ies) that contain(s) the data types instead of registering the types one by one.

```c#
[ScanForPropertyComparison]
public class MyModel { ... }

[ScanForPropertyComparison]
public class MyModelVM { ... }

//Etc.  Any and all data types to be scanned.  Structs are allowed as well.
```

This is how you scan an entire assembly.  Registration will only take place for those data types appropriately annotated with the attribute:

```c#
public static void Main(string[] args)
{
    Scanner.ScanAssembly(Assembly.GetExecutingAssembly());
}
```

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

Con esta biblioteca cualquier desarrollador puede fácilmente comparar objetos de la misma clase u objetos de clases completamente diferentes propiedad por propiedad.

Existen dos áreas principales donde un comparador así es útil:  Pruebas unitarias de objetos mapeadores, y aplicaciones que dependen fuertemente de modelos de datos.  Puede usarse para determinar rápidamente cambios entre versiones de un registro, o tomar decisiones acerca de las diferencias en datos entre un modelo y su correspondiente modelo-vista, por ejemplo.

### Cómo Funciona

La funcionalidad más básica se basa en nombres de propiedades:  Propiedades en los objetos son pareadas unas con otras si su nombre es igual.  El mapeo (pareo) es sensible a las mayúsculas.  Sin embargo esta biblioteca permite el mapeo personalizado a través del atributo `PropertyMapAttribute` o utilizando la **sintaxis fluida de configuración**.

La comparación propiedad por propiedad utiliza implementaciones de la interfaz `IComparer`.  También puede proveerse objetos comparadores personalizados para un tipo de datos, ya sea directamente a la clase `Scanner` para uso global, o bien directamente a un objeto compararador para uso local.

En el caso de .Net Framework v4.6.1 o superior, esta biblioteca soporta el uso de los diferentes atributos aplicados al tipo de datos configurado con el atributo `MetadataTypeAttribute`.

### Cómo Instalar

La forma más simple es vía NuGet.  El paquete está publicado @ [wj.ObjectComparer](https://www.nuget.org/packages/wj.ObjectComparer).  Si no, entonces el código fuente puede compilarse y referenciar el DLL resultante, o bien puede descargar el archivo zip desde la página de [releases](../../releases) que contiene el DLL compilado.

### Inicio Rápido

Esta biblioteca puede usarse en muchas situaciones.  Las siguientes subsecciones dan los pasos desde el caso más simple hasta el más complejo.

**IMPORTANTE**:  Este inicio rápido trata de cubrir los casos más comunes, pero otros casos son posibles.  También asegúrese de leer el **Caso A** independientemente de su caso particular ya que el código en este caso no se repite en los otros casos.

#### Caso A:  Comparación Simple

**Premisas**

1. Los objetos a comparar son del mismo tipo, o son de tipos diferentes pero los nombres de propiedad corresponden exactamente.
2. Los tipos de datos son conocidos.

**Pasos:**

1. Registe (o escanee) los tipos de datos que están involucrados en la comparación propiedad por propiedad, una vez en el ciclo de vida de la aplicación.
2. Cree un objeto compararador de objetos para los tipos a comparar.  El orden es importante, así que escoja sabiamente.
3. Usando el objeto comparador de objetos, ejecue cualquiera de los métodos `Compare()` y capture el resultado.
4. Si utilizó una de las sobrecargas que retornan una colección, entonces puede examinar los resultados individuales de propiedades.

```c#
//El registro de un tipo de datos se hace solamente una vez por ciclo de vida de la aplicación, como en un método Main() u OnStart().
public static void Main(string[] args)
{
    Scanner.RegisterType<MyModel>();
    Scanner.RegisterType<MyModelVM>();
}

//Ya pueden crearse objetos comparadores de objetos para cualquier combinación de los tipos de datos registrados.
//Compare objetos del mismo tipo:
ObjectComparer oc1 = ObjectComparer.Create<MyModel>();
//Compare objetos de distintos tipos:
ObjectComparer oc2 = ObjectComparer.Create<MyModel, MyModelVM>();
//Etc.  Puede hacerse para MyModelVM -> MyModel, o dos MyModelVM.

//Compare objetos.
MyModel m1 = ObtainModelSomehow();
MyModel m2 = ObtainModelSomehow();
MyModelVM vm = CreateVMSomehow();
var resultsMtoM = oc1.Compare(m1, m2, out bool mToMIsDifferent);
var resultsMtoVM = oc2.Compare(m1, vm, out bool mToVMIsDifferent);
//Los Booleanos de salida dan un valor general del resultado sin la necesidasd de recorrer la colección de resultados de propiedades.
Logger.Information($"La comparación modelo a modelo dice que los objetos son {(mToMIsDifferent ? "diferentes" : "iguales")}.");
Logger.Information($"La comparación modelo a modelo-vista dice que los objetos son {(mToVMIsDifferent ? "diferentes" : "iguales")}.");
//O usar un bucle para recorrer la colección de resultados de propiedades o acceder a un resultado usando
//el nombre de la propiedad.
foreach (PropertyComparisonResult pcr in resultsMtoM)
{
    //La clase PropertyComparisonResult tiene un ToString() personalizado.
    Logger.Information($"{pcr}");
}
//Examinación de un resultado individual:
PropertyComparisonResult pcrSomeProperty = resultsMtoM[nameof(MyModel.SomeProperty)];
if (pcrSomeProperty.Result == ComparisonResult.LessThan)
{
    //Tal vez mostrar el dato en un fondo verde.
}
else if (pcrSomeProperty.Result == ComparisonResult.GreaterThan)
{
    //Tal vez mostrar el dato en un fondo rojo.
}
else if (pcrSomeProperty.Result == ComparisonResult.PropertyIgnored)
{
    //Tal vez quiera grabar esto en una bitácora o algo.
}
else if ((pcrSomeProperty.Result & ComparisonResult.Equal) == ComparisonResult.Undefined)
{
    //Cubrir problemas potenciales como propiedad no encontrada en el objeto 2, o una excepción durante la comparación.
    if ((pcrSomeProperty.Result & ComparisonResult.PropertyNotFound) == ComparisonResult.PropertyNotFound)
    {
        //Propiedad no encontrada en objeto 2.  Sólo pasa cuando se comparan objetos de diferente tipo.
    }
    else if ((pcrSomeProperty.Result & ComparisonResult.Exception) == ComparisonResult.Exception)
    {
        //Ocurrió una excepción durante la comparación.
    }
    else if ((pcrSomeProperty.Result & ComparisonResult.NoComparer) == ComparisonResult.NoComparer)
    {
        //El tipo de datos de la propiedad no implementa IComparable.  Considere crear un IComparer
        //para este tipo de datos y regístrelo con el escáner o con el objeto comparador.
    }
}
```

#### Caso B.1:  Se Necesita un Mapa de Propiedad

**Premisas:**

1. Los objetos a comparar son de tipos diferentes.
2. Los tipos de datos son conocidos.
3. Al menos una propiedad en la fuente tiene un nombre diferente en el destino.
4. El mapeo de la propiedad se necesita en todas partes de la aplicación (globalmente).
5. El código fuente del tipo de datos fuente está disponible para modificación.

**Pasos:**

1. Agregue el atributo `PropertymapAttribute` a cada propiedad en el tipo de datos fuente que tiene un nombre diferente en el tipo de datos destino.
2. Registe (o escanee) los tipos de datos que están involucrados en la comparación propiedad por propiedad, una vez en el ciclo de vida de la aplicación.
3. Cree un objeto compararador de objetos para los tipos a comparar.  El orden es importante, así que escoja sabiamente.
4. Usando el objeto comparador de objetos, ejecue cualquiera de los métodos `Compare()` y capture el resultado.
5. Si utilizó una de las sobrecargas que retornan una colección, entonces puede examinar los resultados individuales de propiedades.

```c#
//This is how a property map is done when using attributes.
public class MyModel
{
    [PropertyMap(typeof(MyModelVM), PropertyMapOperation.MapToProperty, nameof(MyModelVM.DateOfBirth))]
    public DateTime BirthDate { get; set; }
}
```

**NOTA:**  Los mapas de propiedades *no* son bidireccionales.  Solamente porque en el ejemplo arriba existe un mapa MyModel.BirthDate -> MyModelVM.DateOfBirth, la inversa no es cierta.  No existe un mapeo automático a la inversa (MyModelVM.DateOfBirth -> MyModel.BirthDate).

#### Caso B.2:  Se Necesita un Mapa de Propiedad

**Premisas:**

1. Los objetos a comparar son de tipos diferentes.
2. Los tipos de datos son conocidos.
3. Al menos una propiedad en la fuente tiene un nombre diferente en el destino.
4. El mapeo de la propiedad se necesita en todas partes de la aplicación (globalmente).
5. El código fuente del tipo de datos fuente no está disponible para modificación o no se desea modificarle.

**Pasos:**

1. Configure el tipo de datos fuente una vez por ciclo de vida de la aplicación con el mapa de propiedad necesario usando el método `MapProperty()`.
2. Cree un objeto compararador de objetos para los tipos a comparar.  El orden es importante, así que escoja sabiamente.
3. Usando el objeto comparador de objetos, ejecue cualquiera de los métodos `Compare()` y capture el resultado.
4. Si utilizó una de las sobrecargas que retornan una colección, entonces puede examinar los resultados individuales de propiedades.

```c#
public static void Main(string[] args)
{
    //El tipo de datos fuente puede o no haber sido pre-registrado.  No tiene importancia.
    Scanner.ConfigureType<MyModel>().ForType<MyModelVM>()
        .MapProperty(src => src.BirthDate, dst => dst.DateOfBirth);
}
```

#### Caso B.3:  Se Necesita un Mapa de Propiedad

**Premisas:**

1. Los objetos a comparar son de tipos diferentes.
2. Los tipos de datos son conocidos.
3. Al menos una propiedad en la fuente tiene un nombre diferente en el destino.
4. El mapeo de la propiedad no se necesita en todas partes de la aplicación (localmente).

**Pasos:**

1. Cree un objeto de configuración de comparador usando el método `ComparerConfigurator.Configure()` apropiado.
2. Agregue el mapa de propiedasd necesario usando el método `MapProperty()` del objeto de configuración.
3. Cree el objeto comparador de objetos para los tipos a comparar mediante el método `CreateComparer()` del objeto de configuración.
4. Usando el objeto comparador de objetos, ejecue cualquiera de los métodos `Compare()` y capture el resultado.
5. Si utilizó una de las sobrecargas que retornan una colección, entonces puede examinar los resultados individuales de propiedades.

```c#
ObjectComparer oc = ComparerConfigurator.Configure<MyModel, MyModelVM>()
    .MapProperty(src => src.BirthDate, dst => dst.DateOfBirth)
    .CreateComparer();
```

#### Caso C.1:  Debe Ignorarse una Propiedad para un Tipo Destino Específico

**Premisas:**

1. Los objetos a comparar son de tipos diferentes.
2. Los tipos de datos son conocidos.
3. Al menos una propiedad en la fuente debe ser ignorada cuando se compara contra objetos del tipo de destino.
4. Ignorar la propiedad se necesita en todas partes de la aplicación (globalmente).
5. El código fuente del tipo de datos fuente está disponible para modificación.

**Pasos:**

1. Agregue el atributo `PropertyMapAttribute` a cada propiedad en el tipo de datos fuente que deba ignorarse cuando se compara contra objetos del tipo de datos de destino.
2. Cree un objeto compararador de objetos para los tipos a comparar.  El orden es importante, así que escoja sabiamente.
3. Usando el objeto comparador de objetos, ejecue cualquiera de los métodos `Compare()` y capture el resultado.
4. Si utilizó una de las sobrecargas que retornan una colección, entonces puede examinar los resultados individuales de propiedades.

```c#
public class MyModel
{
    [PropertyMap(typeof(MyModelVM), PropertyMapOperation.IgnoreProperty)]
    public DateTime BirthDate { get; set; }
}
```

#### Caso C.2:  Debe Ignorarse una Propiedad para un Tipo Destino Específico

**Premisas:**

1. Los objetos a comparar son de tipos diferentes.
2. Los tipos de datos son conocidos.
3. Al menos una propiedad en la fuente debe ser ignorada cuando se compara contra objetos del tipo de destino.
4. Ignorar la propiedad se necesita en todas partes de la aplicación (globalmente).
5. El código fuente del tipo de datos fuente no está disponible para modificación o no se desea modificarle.

**Pasos:**

1. Configure el tipo de datos fuente una vez por ciclo de vida de la aplicación con el mapa de propiedad necesario usando el método `IgnoreProperty()`.
2. Cree un objeto compararador de objetos para los tipos a comparar.  El orden es importante, así que escoja sabiamente.
3. Usando el objeto comparador de objetos, ejecue cualquiera de los métodos `Compare()` y capture el resultado.
4. Si utilizó una de las sobrecargas que retornan una colección, entonces puede examinar los resultados individuales de propiedades.

```c#
public static void Main(string[] args)
{
    Scanner.ConfigureType<MyModel>.ForType<MyModelVM>()
        .IgnoreProperty(src => src.BirthDate);
}
```

#### Caso C.3:  Debe Ignorarse una Propiedad para un Tipo Destino Específico

**Premisas:**

1. Los objetos a comparar son de tipos diferentes.
2. Los tipos de datos son conocidos.
3. Al menos una propiedad en la fuente debe ser ignorada cuando se compara contra objetos del tipo de destino.
4. Ignorar la propiedad no se necesita en todas partes de la aplicación (localmente).

**Pasos:**

1. Cree un objeto de configuración de comparador usando el método `ComparerConfigurator.Configure()` apropiado.
2. Ignore la propiedad usando el método `IgnoreProperty()` del objeto de configuración.
3. Cree el objeto comparador de objetos para los tipos a comparar mediante el método `CreateComparer()` del objeto de configuración.
4. Usando el objeto comparador de objetos, ejecue cualquiera de los métodos `Compare()` y capture el resultado.
5. Si utilizó una de las sobrecargas que retornan una colección, entonces puede examinar los resultados individuales de propiedades.

```c#
ObjectComparer oc = ComparerConfigurator.Configure<MyModel, MyModelVM>()
    .IgnoreProperty(src => src.BirthDate)
    .CreateComparer();
```

#### Caso D.1:  Debe Ignorarse una Propiedad para Tipos Destino No Especificados

**Premisas:**

1. Los objetos a comparar son del mismo tipo o de tipos diferentes.
2. Los tipos de datos destino son desconocidos o no se deseas especificarles, o el tipo de datos de destino es el mismo que el tipo de datos fuente.
3. Al menos una propiedad en la fuente debe ser ignorada cuando se compara contra objetos del tipo de destino.
4. Ignorar la propiedad se necesita en todas partes de la aplicación (globalmente).
5. El código fuente del tipo de datos fuente está disponible para modificación.

**Pasos:**

1. Agregue el atributo `IgnoreForComparisonAttribute` a cada propiedad en el tipo de datos fuente que debe ignorarse usando el valor apropiado.
2. Registe (o escanee) los tipos de datos que están involucrados en la comparación propiedad por propiedad, una vez en el ciclo de vida de la aplicación.
3. Cree un objeto compararador de objetos para los tipos a comparar.  El orden es importante, así que escoja sabiamente.
4. Usando el objeto comparador de objetos, ejecue cualquiera de los métodos `Compare()` y capture el resultado.
5. Si utilizó una de las sobrecargas que retornan una colección, entonces puede examinar los resultados individuales de propiedades.

```c#
public class MyModel
{
    [IgnoreForComparison]
    public DateTime IgnoredForOtherDataTypes { get; set; }

    [IgnoreForComparison(IgnorePropertyOptions.IgnoreForSelf)]
    public DateTime IgnoredForSelfDataType { get; set; }

    [IgnoreForComparison(IgnorePropertyOptions.IgnoreForAll)]
    public DateTime IgnoredForAllDataTypes { get; set; }
}
```

#### Caso D.2:  Debe Ignorarse una Propiedad para Tipos Destino No Especificados

**Premisas:**

1. Los objetos a comparar son del mismo tipo o de tipos diferentes.
2. Los tipos de datos destino son desconocidos o no se deseas especificarles, o el tipo de datos de destino es el mismo que el tipo de datos fuente.
3. Al menos una propiedad en la fuente debe ser ignorada cuando se compara contra objetos del tipo de destino.
4. Ignorar la propiedad se necesita en todas partes de la aplicación (globalmente).
5. El código fuente del tipo de datos fuente no está disponible para modificación o no se desea modificarle.

**Pasos:**

1. Configure el tipo de datos fuente una vez por ciclo de vida de la aplicación usando el método `IgnoreProperty()`.
2. Cree un objeto compararador de objetos para los tipos a comparar.  El orden es importante, así que escoja sabiamente.
3. Usando el objeto comparador de objetos, ejecue cualquiera de los métodos `Compare()` y capture el resultado.
4. Si utilizó una de las sobrecargas que retornan una colección, entonces puede examinar los resultados individuales de propiedades.

```c#
public static void Main(string[] args)
{
    Scanner.ConfigureType<MyModel>
        .IgnoreProperty(src => src.BirthDate, IgnorePropertyOptions.IgnoreForAll);
}
```

### Más Acerca de la Configuración Basada en Atributos

Si se tienen muchos tipos de datos a escanear, podría ser mejor marcarles con el atributo `ScanForPropertyComparisonAttribute` y decirle al escáner que escanee el(los) ensamblado(s) que contiene(n) el(los) tipo(s) de datos en lugar de registrar cada tipo uno por uno.

```c#
[ScanForPropertyComparison]
public class MyModel { ... }

[ScanForPropertyComparison]
public class MyModelVM { ... }

//Etc.  Todos los tipos de datos a escaner.  También es posible escanear estructuras (struct).
```

Así es como se escanea un ensamblado completo.  El registro de un tipo se dará únicamente para aquelos tipos anotados apropiadamente con el atributo:

```c#
public static void Main(string[] args)
{
    Scanner.ScanAssembly(Assembly.GetExecutingAssembly());
}
```

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
