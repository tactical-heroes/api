# Архитектурные тесты

Проект содержит автоматические ограничения на зависимости между слоями и
модулями, устройство доменной модели, соглашения Application, Infrastructure и
Presentation, а также единый формат тестов.

Запуск из корня репозитория:

```powershell
dotnet test tests/PANiXiDA.TacticalHeroes.ArchitectureTests/PANiXiDA.TacticalHeroes.ArchitectureTests.csproj
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

## Domain

В папках и пространствах имён `Abstractions` слоя Domain располагаются только
абстракции. Интерфейс репозитория конкретного агрегата, наследующий
`IRepository<,>`, является доменным контрактом и должен находиться в
`Domain/<Aggregates>/Abstractions`. Предметные параметры и результаты такого
репозитория не должны быть примитивами: для идентификаторов используются strongly
typed ID, а для состояния — aggregate root и другие доменные типы.

9. `AggregateRoots_Should_ContainOnlyDomainTypes_When_StateIsDeclared` — поля
   aggregate root могут содержать только value object, strongly typed ID,
   `Enumeration<>` или entity. Для коллекций проверяется тип элемента.

10. `Entities_Should_ContainOnlyDomainTypes_When_StateIsDeclared` — поля entity,
    не являющихся aggregate root, могут содержать только value object, strongly
    typed ID или `Enumeration<>`. Вложенные entity для них запрещены.

11. `DomainTypes_Should_HaveMatchingUnitTestFiles_When_DomainTypesAreDeclared` —
    каждый aggregate root, entity, value object, strongly typed ID и
    `Enumeration<>` должен иметь отдельный файл unit-тестов. Путь файла повторяет
    модуль, относительный namespace и имя доменного типа.

12. `DomainUnitTests_Should_CoverEveryAccessibleMethod_When_DomainMethodsAreDeclared`
    — для каждого публичного, internal или protected internal доменного метода
    должен существовать тестовый метод с префиксом
    `<ИмяМетода>_Should_`. Для перегрузок требуется соответствующее количество
    тестовых методов.

13. `AggregateRootsAndEntities_Should_NotDeclarePublicSetters_When_DomainStateIsDeclared`
    — свойства aggregate root и entity не должны иметь публичные setter.
    Изменение состояния разрешено только через контролируемое поведение
    доменной модели.

14. `AggregateRootsAndEntities_Should_NotExposeMutableCollections_When_PublicStateIsDeclared`
    — публичные поля, свойства и возвращаемые значения методов aggregate root и
    entity не должны раскрывать массивы, `ICollection<>`, `IList<>`,
    `IDictionary<,>`, `ISet<>` или их реализации. В том числе проверяются
    изменяемые коллекции, вложенные в generic-обёртки.

15. `AggregateRoots_Should_NotContainOtherAggregateRoots_When_StateIsDeclared` —
    поля aggregate root не должны содержать другой aggregate root напрямую,
    через массив или через generic-коллекцию.

16. `ConcreteDomainClasses_Should_BeSealed_When_Declared` — каждый конкретный
    класс в сборках `.Domain`, включая aggregate root, entity, value object,
    enumeration и domain event, должен быть `sealed`.

17. `AggregateRoots_Should_HaveSingularNamesAndPluralDirectories_When_Declared`
    — имя типа aggregate root должно быть в единственном числе, а его namespace
    и физическая папка — во множественном. Например, aggregate root `User`
    размещается в `Users/User.cs`, а `Faction` — в `Factions/Faction.cs`.

18. `Entities_Should_HaveSingularNamesAndPluralDirectories_When_Declared` — имя
    типа entity должно быть в единственном числе. Entity размещается под своим
    aggregate root или родительской entity по пути
    `Entities/<EntityName во множественном числе>/<EntityName>.cs`. Например,
    `UserClaim` размещается в `Users/Entities/UserClaims/UserClaim.cs`.

19. `ValueObjects_Should_ResideInOwnerValueObjectsDirectories_When_Declared` —
    каждый value object должен находиться в папке и namespace `ValueObjects`
    непосредственно под владеющим aggregate root или entity. Например,
    `UserName` находится в `Users/ValueObjects`, а `ClaimType` для `UserClaim` —
    в `Users/Entities/UserClaims/ValueObjects`.

20. `Enumerations_Should_ResideInOwnerEnumerationsDirectories_When_Declared` —
    каждый `Enumeration<>` должен находиться в папке и namespace `Enumerations`
    непосредственно под владеющим aggregate root или entity. Например,
    `UserStatus` находится в `Users/Enumerations`.

21. `DomainEvents_Should_ResideInEventsDirectories_When_Declared` — каждый
    domain event должен находиться в папке и namespace `Events` своей доменной
    области или владельца. Например, события `User` находятся в
    `Users/Events`.

22. `StronglyTypedIds_Should_MatchOwnerNamesAndLocations_When_Declared` —
    strongly typed ID каждого aggregate root или entity называется
    `<OwnerName>Id`, реализует `IStronglyTypedId` и лежит в той же папке и
    namespace, что и владелец. Например, `UserId` лежит рядом с `User`, а
    `UserClaimId` — рядом с `UserClaim`. Неиспользуемые strongly typed ID
    запрещены.

## Application

### Vertical Slice

1. Все use case агрегата располагаются в его feature-папке, название которой
   записывается во множественном числе, например `Factions` или `Users`.

2. Внутри feature-папки агрегата каждый use case располагается в отдельной
   подпапке с названием конкретной фичи, например `Create` или `GetDetails`.
   Имена типов строятся из названия фичи, названия агрегата и роли типа:
   `CreateFactionCommand`, `CreateFactionHandler`.

3. В папках и пространствах имён `Abstractions` слоёв Domain и Application
   располагаются только абстракции. Наследник `IRepository<,>` находится в
   Domain, а наследник `IReadRepository<>` — в Application.

4. Реализация `ICommand` должна оканчиваться на `Command`, реализация `IQuery` —
   на `Query`, а реализации `ICommandHandler<,>` и `IQueryHandler<,>` — на
   `Handler`. Реализация `AbstractValidator<T>` называется по шаблону
   `<ValidatedType>Validator`, например `CreateFactionCommandValidator`.

5. `Command`, его `Handler` и `Validator` располагаются в одной общей
   feature-папке. То же правило действует для `Query`, его `Handler` и
   `Validator`.

6. `IRepository<,>` и его наследники не используют примитивы в предметных
   контрактах. Для `IReadRepository<>` используются примитивные идентификаторы и
   параметры; aggregate root не должен быть его параметром типа, аргументом или
   результатом.

### Автоматические проверки

23. `RepositoryInterfaces_Should_MatchFeatureAndAbstraction_When_Declared` —
    интерфейс, наследующий `IRepository<,>`, должен называться
    `I<Feature>Repository`, а наследующий `IReadRepository<>` —
    `I<Feature>ReadRepository`. `<Feature>` берётся из сегмента namespace
    непосредственно перед `.Abstractions`.

24. `ConstructorParameters_Should_FollowTypeBasedNaming_When_AggregateRepositoryIsInjected`
    — параметр конструктора типа aggregate repository именуется по типу
    интерфейса без начальной `I` и с маленькой первой буквы:
    `IFactionsRepository` превращается в `factionsRepository`.

25. `ConstructorParameters_Should_FollowTypeBasedNaming_When_ReadRepositoryIsInjected`
    — параметр конструктора типа read repository строится по тому же правилу:
    `IFactionsReadRepository` превращается в `factionsReadRepository`.

26. `ApplicationHandlers_Should_HaveMatchingUnitTestFiles_When_HandlersAreDeclared`
    — каждый конкретный `ICommandHandler<,>`, `IQueryHandler<,>` или
    `IEventHandler<>` в Application должен иметь отдельный файл unit-тестов.
    Путь файла повторяет модуль, относительный namespace и имя handler.

27. `ApplicationHandlerUnitTests_Should_CoverEveryHandlerMethod_When_HandlersAreDeclared`
    — для каждого метода реализуемого handler-контракта должен существовать
    тестовый метод с префиксом `<ИмяМетода>_Should_`. Для перегрузок учитывается
    количество методов с одинаковым именем.

28. `CommandAndQueryHandlers_Should_HaveValidators_When_HandlersAreDeclared` —
    request каждого command или query handler должен иметь реализацию
    `IValidator<TRequest>`. Для event handler validator не требуется.

29. `ApplicationValidators_Should_HaveMatchingUnitTestFiles_When_ValidatorsAreDeclared`
    — каждая конкретная реализация `IValidator<T>` в Application должна иметь
    отдельный непустой файл unit-тестов по пути, соответствующему её модулю,
    namespace и имени.

30. `ApplicationHandlers_Should_BeSealed_When_Declared` — каждый конкретный
    command, query или event handler в сборках `.Application` должен быть
    `sealed`.

## Infrastructure

31. `RepositoryImplementations_Should_MatchInterfaceNames_When_Declared` —
    каждый интерфейс репозитория должен иметь ровно одну конкретную реализацию в
    Infrastructure. Имя реализации совпадает с именем интерфейса без начальной
    `I`: например, `IFactionsRepository` реализуется классом
    `FactionsRepository`.

32. `RepositoryImplementations_Should_BeSealed_When_Declared` — каждый
    конкретный класс Infrastructure, реализующий `IRepository<,>` или
    `IReadRepository<>`, должен быть `sealed`.

33. `InfrastructureImplementations_Should_HaveMatchingIntegrationTestFiles_When_ApplicationInterfacesAreImplemented`
    — каждый конкретный класс Infrastructure, реализующий интерфейс из
    Application своего модуля, должен иметь отдельный integration-test файл.
    Путь повторяет относительный namespace и имя реализации.

34. `IntegrationTests_Should_CoverEveryApplicationInterfaceMethod_When_ImplementationExists`
    — для каждого метода реализуемого Application-интерфейса должен существовать
    integration-тест с префиксом `<ИмяМетода>_Should_`. Для перегрузок
    учитывается количество методов с одинаковым именем.

## Presentation

35. `Endpoints_Should_HaveMatchingFunctionalTestFiles_When_Declared` — каждый
    конкретный `IEndpoint` должен иметь functional-test файл в том же модуле.
    Путь повторяет относительный namespace и имя endpoint.

36. `EndpointMetadata_Should_FollowNamingConventions_When_EndpointIsDeclared` —
    `Route` endpoint и endpoint group состоит из английских lowercase
    kebab-case сегментов и параметров вида `{name}` или `{name:constraint}`;
    `Name` является одним английским PascalCase-идентификатором; `Summary`
    endpoint записывается на английском в sentence case с одиночными пробелами.

37. `EndpointsAndGroups_Should_BeSealed_When_Declared` — каждый конкретный
    `IEndpoint` и `IEndpointGroup` в сборках `.Presentation` должен быть
    `sealed`.

## Оформление тестов

38. `FactsAndTheories_Should_DeclareDisplayName_When_ATestIsDeclared` — каждый
    `[Fact]` и `[Theory]` во всех тестовых проектах должен содержать
    `DisplayName`, заданный строковым литералом.

39. `DisplayNames_Should_DescribeTestCondition_When_ATestIsDeclared` —
    `DisplayName` записывается на английском по схеме
    `<subject> should <behavior> when <condition>`. Часть после `when` должна
    соответствовать условию из имени тестового метода после `_When_`.

40. `TestMethods_Should_FollowNamingConvention_When_ATestIsDeclared` — имя
    каждого тестового метода должно соответствовать шаблону
    `MethodName_Should_DoSomething_When_Condition`.

41. `TestMethods_Should_FollowArrangeActAssert_When_ATestIsDeclared` — тест
    должен иметь block body, как минимум две логические секции, разделённые
    пустой строкой, и assertion в последней секции.

Пункты 12, 27 и 34 проверяют наличие соответствующих тестовых методов по их
именам, а не факт выполнения production-кода. Фактическое покрытие измеряется
отдельно средствами code coverage в CI.
