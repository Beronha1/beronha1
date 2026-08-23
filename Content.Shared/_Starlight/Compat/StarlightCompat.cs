// SPDX-License-Identifier: MIT
// Shadekin: ported from ss14Starlight/space-station-14 (MIT).
//
// O Shadekin nasceu dentro do ecossistema do Starlight e conversa com subsistemas
// que a Whiskey não tem: Railroading, Medical.Surgery, Language, Overlay e outros.
// Em quase todos, o acoplamento é uma ou duas linhas, e portar o subsistema
// inteiro custaria centenas de arquivos para ganhar nada.
//
// Este arquivo declara os tipos que faltam, com o mesmo nome e namespace do
// original, para o código do Shadekin compilar sem ser reescrito. Evento que
// ninguém dispara é tratador que nunca roda: a funcionalidade fica inerte, e não
// quebrada. Onde a Whiskey tem equivalente de verdade, como os eventos de órgão,
// o certo é ligar no evento dela em vez de usar o coto daqui.

using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Shared._Starlight.Bluespace
{
    /// <summary> Sinal de que a entidade foi empurrada para fora do nullspace. </summary>
    [ByRefEvent]
    public record struct NullSpaceShuntEvent;
}

namespace Content.Shared._Starlight.Medical.Body.Events
{
    /// <summary> Órgão entrou no corpo. Ver OrganGotInsertedEvent da Whiskey. </summary>
    [ByRefEvent]
    public record struct OrganAddedToBodyEvent(EntityUid Body);
}

namespace Content.Shared._Starlight.Medical.Surgery.Events
{
    /// <summary> Órgão retirado em cirurgia. Ver OrganGotRemovedEvent da Whiskey. </summary>
    [ByRefEvent]
    public record struct SurgeryOrganExtracted(EntityUid Body);

    /// <summary> Órgão implantado em cirurgia. </summary>
    [ByRefEvent]
    public record struct SurgeryOrganImplantationCompleted(EntityUid Body);
}

namespace Content.Shared._Starlight.Overlay.Components
{
    /// <summary> Marca quem recebe sobreposição visual. Inerte aqui. </summary>
    [RegisterComponent, NetworkedComponent]
    public sealed partial class StarlightOverlayComponent : Component;
}

namespace Content.Shared._Starlight.Station
{
    /// <summary> Evento de estação do Starlight. Inerte aqui. </summary>
    [ByRefEvent]
    public record struct ForcedPrototypeDoSpecialEvent;
}


// O coto de CosmicCultComponent foi removido: ele registrava o nome "CosmicCult",
// que a Whiskey JÁ usa em Content.Trauma.Shared.CosmicCult.Components, e componente
// registrado duas vezes derruba o jogo no arranque. O culto cósmico daqui é o de
// verdade, e quem liga ele nas regras da escuridão é o CultoCosmicoNaEscuridaoSystem,
// em Content.Trauma.Shared, que é o único lugar que enxerga os dois lados.

namespace Content.Shared._Starlight.Language.Systems
{
    /// <summary>
    ///     O Shadekin declara dependência disto e nunca usa. Mantido só para o
    ///     campo compilar; pode sair junto com a linha que o declara.
    /// </summary>
    public sealed partial class SharedLanguageSystem : EntitySystem;
}

namespace Content.Shared._Starlight.Railroading
{
    /// <summary>
    ///     Sistema de tarefas do Starlight. O Shadekin faz uma chamada só, ao
    ///     abrir portal. Sem tarefa nenhuma registrada, não fazer nada é o
    ///     comportamento correto.
    /// </summary>
    public sealed partial class RailroadingSupercritPortalSystem : EntitySystem
    {
        public void SupercriticalTask(Entity<Component?> ent) { }
        public void SupercriticalTask(EntityUid ent) { }
    }
}

namespace Content.Shared.Teleportation.Components
{
    /// <summary>
    ///     O Starlight acrescentou este evento ao PortalComponent base para deixar
    ///     outros sistemas vetarem um teleporte. A Whiskey não tem, e o
    ///     DarkBreacher do Shadekin assina ele para impedir portal enquanto está
    ///     na escuridão.
    ///
    ///     Fica aqui em vez de no arquivo base para todo o remendo do porte viver
    ///     num lugar só. Enquanto o PortalSystem da Whiskey não levantar este
    ///     evento, o veto do DarkBreacher não tem efeito, e isso é aceitável: o
    ///     resto da raça funciona.
    /// </summary>
    public sealed class OnAttemptPortalEvent : CancellableEntityEventArgs
    {
        public EntityUid Subject { get; }

        public OnAttemptPortalEvent(EntityUid subject)
        {
            Subject = subject;
        }
    }
}

namespace Content.Shared._Starlight.Railroading.Components.Tasks
{
    /// <summary> Tarefa do sistema de objetivos do Starlight. Inerte aqui. </summary>
    [RegisterComponent]
    public sealed partial class RailroadSupercritPortalTaskComponent : Component;

    /// <summary>
    ///     Avisa que uma lâmpada quebrou. No Starlight isto alimenta a tarefa de
    ///     quebrar luz do Railroading. Aqui o Shadegen levanta e ninguém escuta,
    ///     que é o comportamento certo enquanto não existir o sistema de tarefas.
    /// </summary>
    public sealed class OnLightBreakEvent : EntityEventArgs
    {
        /// <summary> A lâmpada que quebrou. </summary>
        public EntityUid Light;

        public OnLightBreakEvent(EntityUid light)
        {
            Light = light;
        }
    }
}

namespace Content.Shared._Starlight.Railroading.Components.Watchers
{
    /// <summary> Observador de tarefa do Starlight. Inerte aqui. </summary>
    [RegisterComponent]
    public sealed partial class RailroadSupercritPortalWatcherComponent : Component;
}
