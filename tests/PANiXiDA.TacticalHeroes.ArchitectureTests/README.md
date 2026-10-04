# Архитектурные тесты

Проект содержит автоматические ограничения на зависимости между слоями и
модулями, устройство доменной модели, соглашения Application, Infrastructure и
Presentation, а также единый формат тестов.

Запуск из корня репозитория:

```powershell
dotnet test --project tests/PANiXiDA.TacticalHeroes.ArchitectureTests/PANiXiDA.TacticalHeroes.ArchitectureTests.csproj
```

## Границы слоёв и модулей

1. `DomainLayer_Should_NotDependOnOuterLayers_When_Validated` — типы слоя
   `Domain` не должны зависеть от `Contracts`, `Application`, `Infrastructure`,
   `Presentation` или `Host`. Разрешены только сам Domain и общие внешние
   доменные абстракции.

2. `ApplicationLayer_Should_DependOnlyOnDomainAndSharedAbstractions_When_Validated`
   — типы слоя `Application` не должны зависеть от `Infrastructure`,
   `Presentation` или `Host`. Они могут использовать Domain, Contracts и общие
   прикладные абстракции.

3. `InfrastructureLayer_Should_NotDependOnPresentationOrHost_When_Validated` —
   типы слоя `Infrastructure` не должны зависеть от `Presentation` или `Host`.

4. `PresentationLayer_Should_NotDependOnDomainInfrastructureOrHost_When_Validated`
   — типы слоя `Presentation` не должны напрямую зависеть от `Domain`,
   `Infrastructure` или `Host`. Взаимодействие с бизнес-логикой выполняется
   через Application.

5. `ContractsLayer_Should_NotDependOnModuleLayersOrHost_When_Validated` — типы
   `Contracts` не должны зависеть от `Domain`, `Application`, `Infrastructure`,
   `Presentation` или `Host`. Контракты остаются независимой границей модуля.

6. `Modules_Should_HaveAllExpectedLayerAssemblies_When_Discovered` — каждый
   обнаруженный модуль должен содержать пять сборок с одинаковым префиксом:
   `.Contracts`, `.Domain`, `.Application`, `.Infrastructure` и
   `.Presentation`.

7. `ModuleLayers_Should_NotDependOnOtherModuleInternals_When_Validated` — типы
   `Domain`, `Application`, `Infrastructure` и `Presentation` одного модуля не
   должны зависеть от внутренних слоёв другого модуля. Межмодульные зависимости
   разрешены только через сборки `.Contracts`.

8. `ModuleProjectReferences_Should_FollowAllowedDependencies_When_Validated` —
   прямые `ProjectReference` модулей должны соответствовать следующему графу:
   `Contracts` и `Domain` не ссылаются на внутренние проекты; `Application`
   ссылается только на свой `Domain` и при необходимости на Contracts;
   `Infrastructure` — на свои `Domain` и `Application`, а также на Contracts;
   `Presentation` — на свой `Application` и Contracts. Ссылка на Contracts
   может вести как в свой, так и в другой модуль.

9. `Modules_Should_HaveHostConfigurationsInModulesDirectory_When_Discovered` —
   каждый обнаруженный модуль должен иметь `<Module>ModuleConfiguration` в
   `Host/Configurations/Modules`.

## Domain

10. `AggregateRoots_Should_ContainOnlyDomainTypes_When_StateIsDeclared` — поля
    aggregate root могут содержать только value object, strongly typed ID,
    `Enumeration<>` или entity. Для коллекций проверяется тип элемента.

11. `Entities_Should_ContainOnlyDomainTypes_When_StateIsDeclared` — поля entity,
    не являющихся aggregate root, могут содержать только value object, strongly
    typed ID или `Enumeration<>`. Вложенные entity для них запрещены.

12. `DomainTypes_Should_HaveMatchingUnitTestFiles_When_DomainTypesAreDeclared` —
    каждый aggregate root, entity, value object, strongly typed ID и
    `Enumeration<>` должен иметь отдельный файл unit-тестов. Путь файла повторяет
    модуль, относительный namespace и имя доменного типа.

13. `DomainUnitTests_Should_CoverEveryAccessibleMethod_When_DomainMethodsAreDeclared`
    — для каждого публичного, internal или protected internal доменного метода
    должен существовать тестовый метод с префиксом
    `<ИмяМетода>_Should_`. Для перегрузок требуется соответствующее количество
    тестовых методов.

14. `AggregateRootsAndEntities_Should_NotExposeWritableState_When_DomainStateIsDeclared`
    — aggregate root и entity не должны предоставлять публичные, internal или
    protected internal setter/init-accessor, изменяемые поля и записываемые
    ref-возвраты. Проверяются также базовые классы и явные реализации интерфейсов.
    Private/protected setter, `readonly` поля и `ref readonly` разрешены;
    состояние меняется через доменные методы.

15. `AggregateRootsAndEntities_Should_NotExposeMutableCollections_When_PublicStateIsDeclared`
    — публичные поля, свойства и возвращаемые значения методов aggregate root и
    entity должны объявлять коллекции через интерфейсы чтения либо стандартные
    read-only, immutable или frozen-типы .NET. Массивы и изменяемые коллекции,
    включая списки, словари, множества, очереди, стеки, `Span<T>` и `Memory<T>`,
    запрещены. `ReadOnlySpan<T>` и `ReadOnlyMemory<T>` допустимы. Проверяются
    также коллекции, вложенные в generic-обёртки.

16. `AggregateRootsAndEntities_Should_ReturnProtectedCollections_When_CollectionsAreExposed`
    — коллекции aggregate root и entity должны возвращаться через защищённое
    представление, запрещающее внешнее добавление, удаление и замену элементов.
    Выражения возврата в исходниках Domain проверяются через Roslyn:
    фактический тип должен быть стандартной read-only обёрткой, immutable- или
    frozen-коллекцией .NET. Поддерживаются списки, словари, множества и защищённые
    представления других коллекций. Приведение внутренней коллекции к интерфейсу
    чтения не считается защитой; проверяются также ветви условных выражений.
    Проверка охватывает public/internal/protected internal члены и явные
    реализации интерфейсов, прямой возврат коллекции как `object`, кортежи,
    вложенные коллекции и generic-обёртки, например `Task<ReadOnlyCollection<T>>`.
    `null`, ветви с `throw` и стандартные преобразования в read-only представления
    памяти разрешены; пользовательские преобразования защитой не считаются.
    Произвольные методы, возвращающие только `IEnumerable` или `IReadOnly...`,
    требуют явной защищённой обёртки. Для `yield return`, включая async-итераторы,
    проверяется защита выдаваемых вложенных коллекций. Элементы-entity могут
    менять состояние через доменные методы; неизменяемость VO и ID проверяется отдельно.

17. `AggregateRoots_Should_NotContainOtherAggregateRoots_When_StateIsDeclared` —
    поля aggregate root не должны содержать другой aggregate root напрямую,
    через массив или через generic-коллекцию.

18. `ConcreteDomainClasses_Should_BeSealed_When_Declared` — каждый конкретный
    класс в сборках `.Domain`, включая aggregate root, entity, value object,
    enumeration и domain event, должен быть `sealed`.

19. `AggregateRoots_Should_HaveSingularNamesAndPluralDirectories_When_Declared`
    — имя типа aggregate root должно быть в единственном числе, а его namespace
    и физическая папка — во множественном. Например, aggregate root `User`
    размещается в `Users/User.cs`, а `Faction` — в `Factions/Faction.cs`.

20. `Entities_Should_HaveSingularNamesAndPluralDirectories_When_Declared` — имя
    типа entity должно быть в единственном числе. Entity размещается под своим
    aggregate root или родительской entity по пути
    `Entities/<EntityName во множественном числе>/<EntityName>.cs`. Например,
    `UserClaim` размещается в `Users/Entities/UserClaims/UserClaim.cs`.

21. `ValueObjects_Should_ResideInOwnerValueObjectsDirectories_When_Declared` —
    каждый value object должен находиться в папке и namespace `ValueObjects`
    непосредственно под владеющим aggregate root или entity. Например,
    `UserName` находится в `Users/ValueObjects`, а `ClaimType` для `UserClaim` —
    в `Users/Entities/UserClaims/ValueObjects`.

22. `ValueObjects_Should_ContainOnlyImmutableState_When_Declared` — все поля
    экземпляра value object должны быть `readonly`, включая приватные поля
    базовых классов. Типы вложенного состояния проверяются рекурсивно по тем же
    ограничениям, что и у доменных событий: запрещены изменяемые поля, массивы,
    изменяемые коллекции и полиморфное состояние. `get` без setter сам по себе
    недостаточен: свойство с `List<T>` или `readonly` ссылка на изменяемый объект
    также нарушают правило. Существующий запрет `set` и `init` для свойств VO
    сохраняется. Записываемые ref-возвраты запрещены.

23. `Enumerations_Should_ResideInOwnerEnumerationsDirectories_When_Declared` —
    каждый `Enumeration<>` должен находиться в папке и namespace `Enumerations`
    непосредственно под владеющим aggregate root или entity. Например,
    `UserStatus` находится в `Users/Enumerations`.

24. `Enumerations_Should_ContainOnlyImmutableState_When_Declared` — экземпляры
    `Enumeration<>` подчиняются той же рекурсивной проверке состояния, что VO:
    все поля экземпляра, включая унаследованные, должны быть `readonly`, а
    вложенные значения — неизменяемыми. Запрет setter/init у свойств сохраняется.

25. `DomainEvents_Should_ResideInEventsDirectories_When_Declared` — каждый
    domain event должен находиться в папке и namespace `Events` своей доменной
    области или владельца. Например, события `User` находятся в
    `Users/Events`.

26. `DomainEvents_Should_ContainOnlyImmutableState_When_Declared` — состояние
    экземпляров конкретных наследников `DomainEvent` должно быть неизменяемым.
    Обычные setter запрещены при любой видимости; `get` и `init` разрешены. Все поля
    экземпляра должны быть `readonly`, включая приватные поля базовых классов.
    Типы вложенного состояния проверяются рекурсивно: разрешены неизменяемые
    скаляры, структуры и sealed-классы с такими же ограничениями. Для коллекций
    разрешены стандартные типы `ImmutableArray`, `ImmutableList`,
    `ImmutableHashSet`, `ImmutableSortedSet`, `ImmutableDictionary`,
    `ImmutableSortedDictionary`, `ImmutableQueue`, `ImmutableStack`, `FrozenSet`
    и `FrozenDictionary`; их элементы, ключи и значения также проверяются
    рекурсивно. Массивы, изменяемые коллекции, builders, read-only обёртки
    и представления памяти запрещены. `AsReadOnly()` отражает изменения исходной
    коллекции и не создаёт неизменяемый снимок для события. Интерфейсы коллекций,
    включая `IReadOnlyCollection<T>`, запрещены: они могут скрывать изменяемую
    реализацию. Полиморфное состояние (`object`, прочие интерфейсы и
    незапечатанные классы) также запрещено.

27. `DomainEvents_Should_RequireImmutableCollectionsAndElements_When_CollectionsAreDeclared`
    проверяет допустимые и недопустимые варианты коллекций на тестовых событиях,
    включая вложенные коллекции и изменяемые элементы, ключи и значения.
    Поскольку анализ состояния общий, те же ограничения коллекций действуют
    и для VO, strongly typed ID и перечислений.

28. `StronglyTypedIds_Should_MatchOwnerNamesAndLocations_When_Declared` —
    strongly typed ID каждого aggregate root или entity называется
    `<OwnerName>Id`, реализует `IStronglyTypedId` и лежит в той же папке и
    namespace, что и владелец. Например, `UserId` лежит рядом с `User`, а
    `UserClaimId` — рядом с `UserClaim`. Неиспользуемые strongly typed ID
    запрещены.

29. `StronglyTypedIds_Should_ContainOnlyImmutableState_When_Declared` —
    рекурсивная проверка запрещает изменяемые поля и вложенное состояние ID,
    включая приватное и унаследованное. Она дополняет проверку getter без
    setter/init и работает для классов и структур.

30. `DomainTypes_Should_ContainOnlyImmutableStaticState_When_AuthoredMembersAreDeclared`
    — статические поля в написанном нами коде Domain должны быть `const` или
    `readonly` с рекурсивно неизменяемым содержимым. Проверяются также backing fields
    статических auto-property и вложенные типы. Статические setter, события и
    записываемые ref-возвраты запрещены. Фабрики разрешены; сгенерированные
    технические кэши в эту проверку не входят.

31. `Repositories_Should_ResideInDomainAbstractions_When_Declared` — интерфейс,
    наследующий `IRepository<,>`, должен находиться в Domain в папке и
    namespace `Abstractions`.

32. `Repositories_Should_UseStronglyTypedIdsAndAggregateRoots_When_Declared` —
    первый generic-параметр `IRepository<,>` должен быть непримитивным strongly
    typed ID, а второй — aggregate root.

33. `RepositoryMethods_Should_UseOnlyDomainTypes_When_Declared` — параметры и
    результаты методов `IRepository<,>` могут содержать только aggregate root,
    value object, `Enumeration<>` и strongly typed ID. Разрешены технические
    обёртки `Task`, `ValueTask`, коллекции, nullable и `CancellationToken`;
    примитивы, entity, domain events и произвольные DTO запрещены.

34. `Repositories_Should_MatchPluralAggregateNames_When_Declared` — интерфейс,
    наследующий `IRepository<,>`, должен называться
    `I<AggregatePlural>Repository` и находиться в одноимённой feature-папке.
    Корректное английское множественное число строится через Humanizer.

35. `ConstructorParameters_Should_FollowTypeBasedNaming_When_RepositoryIsInjected`
    — параметр конструктора типа repository именуется по типу интерфейса без
    начальной `I` и с маленькой первой буквы: `IFactionsRepository`
    превращается в `factionsRepository`.

36. `DomainState_Should_RejectExternalWrites_When_MemberAccessVaries` —
    проверка самого правила инкапсуляции на тестовых типах: setter/init,
    публичные и internal поля, интерфейсы, наследование и ref-возвраты.
    Допустимые private/protected setter, readonly-поля и ref readonly тоже
    проверяются, чтобы правило не запрещало их по ошибке.

37. `ImmutableState_Should_ValidateFieldsRecursively_When_StateShapeVaries` —
    проверка рекурсивного анализа на изменяемых полях, readonly-ссылках на
    изменяемые объекты, приватных полях базового класса, структурах, nullable
    и циклических графах типов, а также на записываемых и read-only ref-возвратах.
    `init` допустим для состояния событий;
    у VO, ID и перечислений он запрещён отдельными правилами свойств.

38. `CollectionExposure_Should_ValidateProtection_When_MemberShapesVary` —
    тестовые исходники проверяют обнаружение утечек коллекций через свойства,
    методы, индексаторы, internal-члены, интерфейсы, object, Task, кортежи,
    итераторы и представления памяти, а также соответствующие защищённые варианты.

39. `CollectionReturns_Should_IdentifyProtectedStorage_When_ExpressionsUseDifferentCollectionTypes`
    — проверка классификации выражений возврата: стандартные обёртки,
    immutable/frozen-коллекции, изменяемые коллекции, builders, преобразования
    и условные выражения, в том числе ветви с `null`.

40. `StaticState_Should_RejectSharedMutation_When_AuthoredMemberShapesVary` —
    проверка правила статического состояния на полях, свойствах, событиях,
    ref-возвратах и вложенных типах. Константы, неизменяемые значения и фабрики
    проверяются как допустимые варианты.

## Application

41. `ApplicationValidators_Should_BePublicAndSealed_When_ValidatorsAreDeclared` —
    все конкретные Application-валидаторы `IValidator<T>`, включая валидаторы
    фильтров и сгенерированные валидаторы сортировки, должны быть `public sealed`.
    Публичность проверяется вместе с доступностью содержащих типов, чтобы
    генератор регистрации Wolverine мог ссылаться на валидаторы из Host.

42. `ApplicationUseCases_Should_ResideInFeatureFolders_When_Declared` — каждый
    `ICommand` и `IQuery` должен находиться в папке конкретной фичи ниже хотя бы
    одной группирующей папки. Между корнем Application и feature-папкой
    разрешено любое количество логических подпапок, например
    `Auth/ChangePassword` или `Users/Administration/Block`.

43. `ApplicationUseCaseTypes_Should_HaveExpectedRoleSuffixes_When_Declared` —
    `ICommand` оканчивается на `Command`, `IQuery` — на `Query`,
    `ICommandHandler<,>` и `IQueryHandler<,>` — на `Handler`, validator — на
    `Validator`.

44. `ApplicationUseCaseParts_Should_ShareOneFeatureFolder_When_Declared` —
    command или query должен иметь ровно один handler и один validator. Request,
    handler и validator именуются согласованно и располагаются в одной общей
    feature-папке и namespace.

45. `AbstractionsNamespaces_Should_ContainOnlyAbstractions_When_Declared` —
    папки и пространства имён `Abstractions` в Domain и Application могут
    содержать только интерфейсы, абстрактные типы и делегаты.

46. `ReadRepositories_Should_ResideInApplicationAbstractions_When_Declared` —
    интерфейс, наследующий `IReadRepository<>`, должен находиться в Application
    в папке и namespace `Abstractions`.

47. `ReadRepositories_Should_UsePrimitiveIds_When_Declared` —
    generic-идентификатор `IReadRepository<>` должен быть примитивом, например
    `Guid`; strongly typed ID и другие доменные идентификаторы запрещены.

48. `ReadRepositoryMethods_Should_UseOnlyPrimitiveInputModels_When_Declared` —
    параметры дополнительных методов read repository могут быть примитивами,
    `CancellationToken`, коллекциями примитивов или Application-моделями,
    публичное состояние которых рекурсивно состоит только из разрешённых типов.
    Любые типы из Domain и `IReadModel` во входных параметрах запрещены.

49. `ReadRepositoryMethods_Should_ReturnReadModels_When_Declared` — каждый
    дополнительный метод read repository должен возвращать реализацию
    `IReadModel`. Допускаются обёртки `Task`, `Result`, коллекции и модели
    пагинации. Базовые методы `ExistsByIdAsync` и `AnyAsync`, возвращающие
    `bool`, к этому правилу не относятся.

50. `ReadModels_Should_EndWithReadModel_When_Declared` — каждый конкретный
    тип, реализующий `IReadModel`, должен оканчиваться на `ReadModel`.

51. `TypesEndingWithReadModel_Should_ImplementIReadModel_When_Declared` — каждый
    конкретный Application-тип с суффиксом `ReadModel` должен реализовывать
    `IReadModel`.

52. `QueryHandlers_Should_ReturnReadModels_When_Declared` — payload результата
    каждого `IQueryHandler<,>` должен реализовывать `IReadModel`. Допускаются
    обёртки `Task`, `Result`, коллекции и модели пагинации.

53. `ReadModels_Should_UseSealedRecordDeclarations_When_Declared` — каждый класс
    или структура слоя Application с суффиксом `ReadModel` или реализацией
    `IReadModel` должен быть объявлен как `sealed record`, без явного `class`
    и без `struct`. Проверка использует семантическую модель Roslyn и учитывает
    унаследованные интерфейсы.

54. `Filters_Should_UseSealedRecordDeclarations_When_Declared` — то же правило
    для всех Application-типов с суффиксом `Filter` или реализацией `IFilter`,
    включая фильтры вне стандартных папок агрегатов.

55. `ReadRepositories_Should_MatchPluralAggregateNames_When_Declared` — интерфейс,
    наследующий `IReadRepository<>`, должен называться
    `I<AggregatePlural>ReadRepository` и находиться в одноимённой
    feature-папке. Корректное английское множественное число строится через
    Humanizer.

56. `ConstructorParameters_Should_FollowTypeBasedNaming_When_ReadRepositoryIsInjected`
    — параметр конструктора типа read repository строится по тому же правилу:
    `IFactionsReadRepository` превращается в `factionsReadRepository`.

57. `ApplicationHandlers_Should_HaveMatchingUnitTestFiles_When_HandlersAreDeclared`
    — каждый конкретный `ICommandHandler<,>`, `IQueryHandler<,>` или
    `IEventHandler<>` в Application должен иметь отдельный файл unit-тестов.
    Путь файла повторяет модуль, относительный namespace и имя handler.

58. `ApplicationHandlerUnitTests_Should_CoverEveryHandlerMethod_When_HandlersAreDeclared`
    — для каждого метода реализуемого handler-контракта должен существовать
    тестовый метод с префиксом `<ИмяМетода>_Should_`. Для перегрузок учитывается
    количество методов с одинаковым именем.

59. `CommandAndQueryHandlers_Should_HaveValidators_When_HandlersAreDeclared` —
    request каждого command или query handler должен иметь реализацию
    `IValidator<TRequest>`. Для event handler validator не требуется.

60. `ApplicationValidators_Should_HaveMatchingUnitTestFiles_When_ValidatorsAreDeclared`
    — каждая конкретная реализация `IValidator<T>` в Application должна иметь
    отдельный непустой файл unit-тестов по пути, соответствующему её модулю,
    namespace и имени. Это включает валидаторы фильтров: тест располагается в
    `Application/<Aggregates>/Common/Filters/<Aggregates>FilterValidatorTests.cs`
    и содержит хотя бы один `[Fact]` или `[Theory]` именно в этом файле.

61. `ApplicationHandlers_Should_BeSealed_When_Declared` — каждый конкретный
    command, query или event handler в сборках `.Application` должен быть
    `sealed`.

62. `AggregateRoots_Should_HaveCommonFilterRecordsAndValidators_When_Declared` —
    каждый агрегат имеет в Application файл
    `<Aggregates>/Common/Filters/<Aggregates>Filter.cs`: `record`, реализующий
    `IFilter`, и соседний `<Aggregates>FilterValidator.cs` с `IValidator<Filter>`.
    Проверяются namespace и фактические объявления типов в этих файлах.

## Infrastructure

63. `RepositoryInterfaces_Should_HaveExactlyOneImplementation_When_Declared` —
    каждый интерфейс, наследующий `IRepository<,>` или `IReadRepository<>`,
    должен иметь ровно одну конкретную реализацию в Infrastructure.

64. `AggregateRoots_Should_HaveRegisteredRepositories_When_Declared` — каждый
    aggregate root должен иметь ровно один repository в корне своей
    `Persistence/Features/<AggregatePlural>/Write` feature и этот repository
    должен быть зарегистрирован в DI модуля.

65. `AggregateRoots_Should_HavePersistenceConfigurations_When_Declared` —
    каждый aggregate root должен иметь отдельную EF Core configuration в корне
    Write feature либо явную inline-конфигурацию соответствующей Identity
    persistence-модели. Итоговая EF-модель write DbContext должна содержать
    настроенный тип.

66. `RepositoryImplementations_Should_UsePluralAggregateNames_When_Declared` —
    каждый наследник `IRepository<,>` должен иметь ровно одну реализацию с
    именем `<AggregatePlural>Repository`, например `FactionsRepository`.

67. `ReadRepositoryImplementations_Should_UsePluralAggregateNames_When_Declared`
    — каждый наследник `IReadRepository<>` должен иметь ровно одну реализацию с
    именем `<AggregatePlural>ReadRepository`, например
    `FactionsReadRepository`.

68. `RepositoryImplementations_Should_ResideInWriteRoots_When_Declared` —
    реализация `IRepository<,>` должна находиться непосредственно в
    `Persistence/Features/<AggregatePlural>/Write`.

69. `ReadRepositoryImplementations_Should_ResideInReadRoots_When_Declared` —
    реализация `IReadRepository<>` должна находиться непосредственно в
    `Persistence/Features/<AggregatePlural>/Read`.

70. `ReadModelComponents_Should_MatchModelNames_When_Declared` — реализации
    `IReadModelMapper<,,>` и `IReadModelSorting<>` должны называться точно
    `<ReadModel>Mapper` и `<ReadModel>Sorting` соответственно.

71. `ReadModelComponents_Should_BeInternalSealedClasses_When_Declared` —
    реализации `IReadModelMapper<,,>` и `IReadModelSorting<>` должны быть
    верхнеуровневыми `internal sealed class`.

72. `ReadModelComponents_Should_ResideInMatchingApplicationSlices_When_Declared`
    — mapper и sorting должны находиться в
    `Persistence/Features/<AggregatePlural>/Read/<Slice>`, где `<Slice>` —
    папка соответствующей ReadModel в Application. Проверяются физические
    пути и namespace.

73. `ReadModelSorting_Should_ShareMapperDirectory_When_Declared` — sorting
    должен находиться в одной папке и namespace с mapper той же ReadModel;
    наличие sorting для одиночной модели не требуется.

74. `ReadDatabaseModels_Should_EndWithReadDbModel_When_Declared` — каждый
    наследник `ReadDbModel<>` или `AuditableReadDbModel<>` должен оканчиваться
    на `ReadDbModel`.

75. `ReadDatabaseModels_Should_ResideInAggregateReadDbModelsDirectories_When_Declared`
    — read database models должны находиться в
    `Persistence/Features/<AggregatePlural>/Read/DbModels`.

76. `AuditableEntityConfigurations_Should_ResideInAggregateWriteRoots_When_Declared`
    — наследники `AuditableEntityConfiguration<>` должны находиться
    непосредственно в `Persistence/Features/<AggregatePlural>/Write`.

77. `EntityTypeConfigurations_Should_ResideInAggregateWriteRoots_When_Declared`
    — реализации `IEntityTypeConfiguration<>` должны находиться непосредственно
    в `Persistence/Features/<AggregatePlural>/Write`.

78. `AuditableEntityConfigurations_Should_AvoidExplicitStoreObjectNames_When_Declared`
    — наследники `AuditableEntityConfiguration<>` не должны явно задавать имена
    таблиц, представлений и столбцов через `ToTable`, `ToView` или
    `HasColumnName`.

79. `EntityTypeConfigurations_Should_AvoidExplicitStoreObjectNames_When_Declared`
    — реализации `IEntityTypeConfiguration<>` подчиняются тому же запрету на
    явные имена таблиц, представлений и столбцов.

80. `ReadDatabaseContexts_Should_MatchModuleNamesAndResideInPersistenceCore_When_Declared`
    — наследник `ReadDbContext<>` называется `<Module>ReadDbContext` и находится
    непосредственно в `Persistence/Core`.

81. `WriteDatabaseContexts_Should_MatchModuleNamesAndResideInPersistenceCore_When_Declared`
    — наследник `WriteDbContext<>` называется `<Module>WriteDbContext` и
    находится непосредственно в `Persistence/Core`.

82. `MigrationsAndModelSnapshots_Should_ResideInPersistenceCoreMigrations_When_Declared`
    — EF Core migrations и model snapshots должны находиться в
    `Persistence/Core/Migrations`.

83. `RepositoryImplementations_Should_BeSealed_When_Declared` — каждый
    конкретный класс Infrastructure, реализующий `IRepository<,>` или
    `IReadRepository<>`, должен быть `sealed`.

84. `InfrastructureImplementations_Should_HaveMatchingIntegrationTestFiles_When_ApplicationInterfacesAreImplemented`
    — каждый конкретный класс Infrastructure, реализующий интерфейс из
    Application своего модуля, должен иметь отдельный integration-test файл.
    Путь повторяет относительный namespace и имя реализации.

85. `IntegrationTests_Should_CoverEveryApplicationInterfaceMethod_When_ImplementationExists`
    — для каждого метода реализуемого Application-интерфейса должен существовать
    integration-тест с префиксом `<ИмяМетода>_Should_`. Для перегрузок
    учитывается количество методов с одинаковым именем.

## Presentation

86. `EndpointGroups_Should_ResideInFeatureRootsAndMatchFeatureNames_When_Declared`
    — каждый конкретный `IEndpointGroup` должен находиться непосредственно в
    `Features/<AggregatePlural>`, называться `<AggregatePlural>Endpoints`, а его
    `Name` должен совпадать с `<AggregatePlural>`.

87. `EndpointGroupMetadataProperties_Should_BeGetOnly_When_GroupIsDeclared` —
    свойства `Route`, `Name` и `ApiVersion` каждого `IEndpointGroup` должны
    предоставлять только getter.

88. `Endpoints_Should_ResideInFeatureSlicesUnderTheirGenericGroups_When_Declared`
    — каждый конкретный `IEndpoint<TGroup>` должен находиться в feature-папке
    внутри дерева своего `TGroup`; между корнем группы и feature-папкой
    допускаются логические подпапки. Generic-параметр обязан указывать на
    `IEndpointGroup` из корня этого дерева.

89. `Endpoints_Should_EndWithEndpoint_When_Declared` — каждый конкретный
    `IEndpoint` должен оканчиваться на `Endpoint`.

90. `MapperlyMappers_Should_EndWithMapper_When_Declared` — каждый mapper,
    объявленный через Mapperly, должен оканчиваться на `Mapper`.

91. `EndpointInputTypes_Should_EndWithRequest_When_Declared` — входной
    Presentation-контракт endpoint должен оканчиваться на `Request`.

92. `EndpointOutputTypes_Should_EndWithResponse_When_Declared` — выходной
    Presentation-контракт endpoint должен оканчиваться на `Response`.

93. `EndpointSliceParts_Should_ShareOneFeatureFolder_When_Declared` —
    `Endpoint`, его `Request`, `Response` и используемые `Mapper` должны
    находиться в одной feature-папке и одном namespace.

94. `CreatedAtRouteCalls_Should_UseEndpointNames_When_Declared` — каждый
    `CreatedAtRoute` должен передавать `routeName` строготипизированно через
    `new <Target>Endpoint().Name`.

95. `MediatorMessages_Should_BeCreatedBySliceMappers_When_EndpointSendsAMessage`
    — endpoint должен обращаться к Application через `IMediator`, а первым
    аргументом фактического `IMediator.SendAsync` или `IMediator.QueryAsync`
    должен быть непосредственный вызов mapper своего slice. Проверка не зависит
    от имени переменной mediator.

96. `Endpoints_Should_HaveMatchingFunctionalTestFiles_When_Declared` — каждый
    конкретный `IEndpoint` должен иметь functional-test файл в том же модуле.
    Путь повторяет относительный namespace и имя endpoint.

97. `EndpointMetadata_Should_FollowNamingConventions_When_EndpointIsDeclared` —
    `Route` endpoint и endpoint group состоит из английских lowercase
    kebab-case сегментов и параметров вида `{name}` или `{name:constraint}`;
    `Name` является одним английским PascalCase-идентификатором; `Summary`
    endpoint записывается на английском в sentence case с одиночными пробелами.

98. `EndpointsAndGroups_Should_BeSealed_When_Declared` — каждый конкретный
    `IEndpoint` и `IEndpointGroup` в сборках `.Presentation` должен быть
    `sealed`.

## Глобальные соглашения

99. `InvocationAndConstructorArguments_Should_BeNamed_When_Ambiguous` —
    аргументы `null`, `default`, `true` и `false`, а также все аргументы вызова
    с тремя и более аргументами в авторских C#-исходниках из `src` должны
    передаваться по имени параметра. Вызовы методов `System.String`, вызовы с
    `params`, `nameof`, EF migrations, `bin`, `obj` и `Generated` не проверяются.

100. `CurrentTimeAccess_Should_UseUtcSources_When_Declared` — текущее время в
    авторских C#-исходниках из `src` должно получаться через
    `TimeProvider.GetUtcNow()`. В явных конструкторах также разрешены
    `DateTime.UtcNow` и `DateTimeOffset.UtcNow` как часть жизненного цикла
    создаваемого объекта. `DateTime.Now`, `DateTime.Today`,
    `DateTimeOffset.Now` и `TimeProvider.GetLocalNow()` запрещены везде. EF
    migrations, `bin`, `obj` и `Generated` не проверяются.

101. `CancellationTokenSentinels_Should_NotBeUsed_When_Declared` — в авторских
    C#-исходниках из `src`, `tests` и `tools` запрещены
    `CancellationToken.None`, `default(CancellationToken)` и `default`, если его
    целевой тип — `CancellationToken`. В том числе токен нельзя объявлять как
    optional-параметр со значением `default`: вызывающий код должен передавать
    фактический токен явно.

102. `CancellationTokenParameters_Should_BeUsed_When_Declared` — объявленный в
     реализованном методе `CancellationToken` должен использоваться; иначе
     параметр нужно удалить. Overrides и реализации внешних интерфейсов не
     проверяются на использование, поскольку удалить параметр из их сигнатуры
     нельзя.

103. `CancellationTokens_Should_BeForwarded_When_Available` — если вызываемый
     метод объявляет параметр `CancellationToken`, а токен уже доступен в текущей
     области видимости, его нужно передать явно.

104. `CancellationTokens_Should_BeAvailable_When_CancellableOperationIsInvoked`
     — если в production-коде из `src` вызываемый метод поддерживает
     `CancellationToken`, но токен не передан и недоступен в текущей области
     видимости, текущий метод должен получить токен параметром. Токен
     последовательно прокидывается по цепочке вызовов от точки входа до
     отменяемой операции.

## Оформление тестов

105. `FactsAndTheories_Should_DeclareDisplayName_When_ATestIsDeclared` — каждый
     `[Fact]` и `[Theory]` во всех тестовых проектах должен содержать
     `DisplayName`, заданный строковым литералом.

106. `DisplayNames_Should_DescribeTestCondition_When_ATestIsDeclared` —
     `DisplayName` записывается на английском по схеме
     `<subject> should <behavior> when <condition>`. Часть после `when` должна
     соответствовать условию из имени тестового метода после `_When_`.

107. `TestMethods_Should_FollowNamingConvention_When_ATestIsDeclared` — имя
     каждого тестового метода должно соответствовать шаблону
     `MethodName_Should_DoSomething_When_Condition`.

108. `TestMethods_Should_FollowArrangeActAssert_When_ATestIsDeclared` — тест
     должен иметь block body, как минимум две логические секции, разделённые
     пустой строкой, и assertion в последней секции.

## Дополнительные архитектурные гарантии

109. `DomainObjects_Should_DeclareOnlyPrivateConstructors_When_CreatedThroughFactories`
     — конкретные `IEntity`, `ValueObject` и `Enumeration<>` должны объявлять
     только private конструкторы. Создание проходит через фабричные методы
     или предопределённые экземпляры перечислений.

110. `InfrastructureImplementations_Should_BeRegisteredForDomainOrApplicationAbstractions_When_Declared`
     — каждая реализация абстракции Domain или Application из Infrastructure
     должна присутствовать в итоговом `IServiceCollection` модуля.

111. `DatabaseContexts_Should_BeRegistered_When_Declared` — каждый конкретный
     `DbContext` должен присутствовать в итоговом `IServiceCollection` модуля.

112. `EndpointMappings_Should_DeclareAuthorizationIntent_When_Mapped` — каждый
     route endpoint должен явно вызвать `RequireAuthorization()` или
     `AllowAnonymous()`, либо наследовать такое решение от своей endpoint group.

113. `EndpointNames_Should_BeUnique_When_Mapped` — итоговые имена endpoint,
     включая явно заданные через `WithName`, должны быть уникальны.

114. `EndpointRoutes_Should_BeUnique_When_Mapped` — сочетание API version, HTTP
     method и полного route endpoint group + endpoint должно быть уникально.

115. `BlockingAsyncCalls_Should_NotBeUsed_When_ProductionCodeIsDeclared` — в
     production-коде запрещены блокирующие вызовы `Task.Wait()`, `Task.Result`
     и `GetAwaiter().GetResult()`.

116. `AsyncVoidCallables_Should_NotBeDeclared_When_ProductionCodeIsDeclared` —
     методы, local functions и lambdas в production-коде не должны быть
     `async void`.

117. `NullForgivingAssignments_Should_TargetOnlyComplexValueObjects_When_UsedInDomainState`
     — присваивание `null!` в состоянии aggregate root и entity разрешено
     только для комплексного value object с несколькими значениями.

118. `CommandValidators_Should_UseDomainFactories_When_DomainConstraintsAreDeclared`
     — command validator не должен повторять бизнес-инварианты через встроенные
     сравнения, диапазоны или произвольные предикаты FluentValidation; проверки
     должны делегироваться доменным фабрикам.

119. `EntityConfigurationLengthLimits_Should_ReferenceDomainConstants_When_Declared`
     — `HasMaxLength` в entity configuration должен ссылаться на публичную
     доменную константу `MaxLength`, а не дублировать числовое ограничение.

120. `ReadDatabaseModelAggregateForeignKeys_Should_HaveBidirectionalNavigations_When_Declared`
     — внешний ключ на aggregate read model из того же модуля требует nullable
     reference navigation у зависимой модели и collection navigation у главной.

121. `EndpointMappings_Should_NotRepeatGroupAuthorization_When_AuthorizationMatches`
     — endpoint не должен повторять тот же `RequireAuthorization` или
     `AllowAnonymous`, который уже объявлен его endpoint group.

122. `ConfigurationOptions_Should_HaveRegisteredValidatorsInSameDirectory_When_Declared`
     — каждый конфигурационный `<Name>Options` с константой `SectionName`
     должен иметь зарегистрированный `<Name>OptionsValidator`, реализующий
     `IValidateOptions<TOptions>` и расположенный рядом с options-классом.

123. `ConfigurationOptions_Should_UseValidateOnStart_When_Registered` — каждый
     конфигурационный options-класс должен регистрироваться через
     `AddOptions<TOptions>()` с последующим вызовом `ValidateOnStart()`.

124. `ConfigurationOptions_Should_ResideInDedicatedOptionsSubdirectories_When_Declared`
     — каждый конфигурационный `<Name>Options` должен располагаться вместе со
     своим validator в выделенной папке `Options/<Name>/`.

125. `AggregateRootsAndEntities_Should_AcceptOnlyDomainTypes_When_MethodsAreDeclared`
     — публичные и internal-методы агрегатов и entity, включая фабрики,
     принимают только VO, strongly typed ID, Enumeration, entity и коллекции
     этих типов. Примитивы и DTO-контейнеры параметров запрещены. Application
     собирает VO через их доменные фабрики до вызова агрегата; фабрики самих
     VO и идентификаторов продолжают принимать и проверять примитивы.

126. `ValueObjectsAndEnumerations_Should_DeclarePublicGettersWithoutSetters_When_PropertiesAreDeclared`
     — свойства VO и `Enumeration<>` должны иметь публичный getter. Любые
     setter и init-accessor запрещены независимо от видимости, включая
     private. Проверяются также статические и унаследованные свойства.

## Пагинация и сортировка

127. `QueryingParameters_Should_UseTypeBasedNames_When_DeclaredOnMethods` —
     параметры методов типа `PaginationParameters` и `SortingParameters`
     должны называться `paginationParameters` и `sortingParameters`.

128. `QueryProperties_Should_UseTypeBasedNames_When_QueryingParametersArePresent`
     — соответствующие свойства Query должны называться `PaginationParameters`
     и `SortingParameters`.

129. `Methods_Should_AcceptSortingParameters_When_PaginationParametersArePresent`
     — метод с параметром `PaginationParameters` обязан принимать и
     `SortingParameters`. Требования возвращать `PaginationResult` нет:
     правило допускает mapper-методы, создающие Query, и endpoints.

130. `Queries_Should_ContainSortingParameters_When_PaginationParametersArePresent`
     — Query со свойством `PaginationParameters` обязана содержать свойство
     `SortingParameters`.

131. `QueryValidators_Should_AttachMatchingChildValidators_When_QueryingParametersArePresent`
     — у Query должен быть validator, подключающий `PaginationParametersValidator`
     к свойству пагинации и `<ReadModel>SortingValidator` к свойству сортировки.
     ReadModel определяется по результату Query; sorting-validator должен
     наследовать `SortingParametersValidator`. Проверяется фактическое подключение
     дочерних validators, а не только наличие их классов.

## Применение маппинга и сортировки в read-репозиториях

132. `ReadRepositoryMethods_Should_ApplyModelSorting_When_ReturningCollections`
     — публичные экземплярные методы и явные реализации интерфейсов
     `IReadRepository<>` в Infrastructure, возвращающие коллекции, должны
     применять `IReadModelSorting<TReadModel>` для типа элемента результата.
     Учитываются обычные коллекции, массивы, `IAsyncEnumerable<>`, результаты
     страничной и курсорной пагинации, обёртки `Task`, `ValueTask` и `Result`.
     Проверяется применение `ApplySorting` либо подходящего sorter через
     `GetPagedResultAsync` в цепочке возвращаемого результата: неиспользуемого
     вызова сортировки недостаточно.

133. `ReadModelSorting_Should_BeNonempty_When_DefaultSortingIsDeclared` —
     реализации `IReadModelSorting<>` должны задавать непустой `DefaultSorting`.
     Наличие `Id` и сортировка по нему не требуются. Уникальность итогового
     порядка этот архитектурный тест не доказывает; для стабильной пагинации
     её нужно обеспечивать подходящими полями конкретной модели и проверять
     интеграционными тестами.

134. `EfReadRepositoryMethods_Should_ApplyMatchingMappers_When_ReturningReadModels`
     — методы EF read-репозиториев, возвращающие ReadModel либо коллекцию,
     должны применять соответствующий `IReadModelMapper<TId, TReadDbModel, TReadModel>`
     через `ProjectTo`, `GetByIdAsync` или `GetPagedResultAsync`.
     Проверяется совпадение всех трёх типов с репозиторием и его результатом,
     а также использование маппинга в цепочке возвращаемого значения.
     Это требование к проекциям EF read-репозиториев, а не ко всем ReadModel
     приложения независимо от способа их создания.

## Поиск через ILIKE

135. `ILikeCalls_Should_UseNamedSubstringArguments_When_Declared` — вызовы
     `ILIKE` должны использовать именованные аргументы `matchExpression`
     и `pattern`, а шаблон поиска — форму `$"%{value.Trim()}%"`.

136. `ILikeCalls_Should_NotUseExplicitNullGuards_When_Declared` — перед `ILIKE`
     не должно быть избыточной явной проверки `matchExpression` на null;
     используется обработка null в SQL.

## Согласованность входа и результата пагинации

137. `ReadRepositories_Should_PairPaginationParametersAndResults_When_MethodsAreDeclared`
     — в контрактах read-репозиториев и их реализациях метод принимает
     `PaginationParameters` тогда и только тогда, когда возвращает
     `PaginationResult<T>`. Проверяются публичные экземплярные методы и явные
     реализации интерфейсов; обёртки `Task`, `ValueTask` и `Result` раскрываются.
     Mapper-методы в этот охват не входят. Наличие `SortingParameters`
     проверяется отдельно правилом 129.

138. `Queries_Should_PairPaginationParametersAndResults_When_Declared` —
     Query содержит свойство `PaginationParameters` тогда и только тогда,
     когда результат её `IQuery<TResult>` — `PaginationResult<T>`, в том числе
     внутри `Result`. Связь с `SortingParameters` проверяется правилом 130.

139. `QueryHandlers_Should_PairQueryPaginationParametersAndResults_When_Declared`
     — у `IQueryHandler<TQuery, TResult>` наличие `PaginationParameters`
     во входной Query должно соответствовать `PaginationResult<T>` в результате
     handler, с раскрытием технических обёрток.

140. `Endpoints_Should_PairPaginationParametersAndResponses_When_Mapped` —
     наличие `PaginationParameters` у обработчика, зарегистрированного через
     `EndpointMapBuilder`, должно совпадать с декларацией
     `Produces<PaginationResult<TResponse>>` и передачей
     `PaginationResult<TResponse>` в `TypedResults.Ok` внутри обработчика.
     Проверяется каждая регистрация маршрута. Это позволяет проверять текущие
     endpoints с возвращаемым типом `Task<IResult>`, не раскрывающим HTTP body.

## Создание strongly typed ID

141. `StronglyTypedIds_Should_DeclareOnlyPrivateConstructors_When_Declared`
     — классы и структуры, реализующие `IStronglyTypedId`, должны объявлять
     только приватные конструкторы. Создание из внешнего значения
     проходит через `Create`, генерация нового идентификатора — через `New`.

142. `StronglyTypedIds_Should_DeclarePublicGettersWithoutSetters_When_PropertiesAreDeclared`
     — свойства strongly typed ID должны иметь публичный getter без setter
     или init-accessor, чтобы нельзя было обойти фабрику через initializer
     или выражение `with`.

## Порядок членов типов

143. `TypeMembers_Should_FollowAgreedOrder_When_Declared` — в авторском коде
     `src`, `tests` и `tools` члены типов идут в порядке: константы, поля,
     конструкторы, финализаторы, события, свойства, индексаторы, методы,
     операторы, вложенные типы. Внутри группы сначала учитывается доступность:
     `public`, `internal`, `protected internal`, `protected`, `private protected`,
     `private`; затем `static` перед экземплярными членами, затем `readonly`
     перед изменяемыми полями. Статический конструктор идёт первым среди
     конструкторов, явные реализации интерфейсов — вместе с публичными членами.
     При равных ключах порядок свободный, сортировка по имени не требуется.
     Каждая часть `partial` проверяется отдельно; элементы enum не сортируются.
     Используется общий поиск исходников без `bin`, `obj`, `Generated` и
     `Migrations`. Проверка работает в тестах, не в `dotnet format`.
     При переносе инициализированных полей и свойств необходимо сохранять
     последовательность вычислений; зависимую инициализацию следует переносить
     в соответствующий конструктор.
