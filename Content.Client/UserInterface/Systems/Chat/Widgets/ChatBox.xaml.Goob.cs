using System.Linq;
using Content.Shared.Chat;

namespace Content.Client.UserInterface.Systems.Chat.Widgets;

public partial class ChatBox
{
    private void ResetCoalescenceState()
    {
        _lastLine = null;
        _lastLineRepeatCount = 0;
    }

    private void UpdateCoalescence(bool value)
    {
        _coalescence = value;
        Repopulate();

        foreach (var child in Contents.Children.ToArray())
        {
            if (child.Name != "_v_scroll")
            {
                Contents.RemoveChild(child);
            }
        }
    }

        public void Repopulate()
        {
            ResetCoalescenceState();
            Contents.Clear();

            // Goobstation start
            foreach (var child in Contents.Children.ToArray())
            {
                if (child.Name != "_v_scroll")
                {
                    Contents.RemoveChild(child);
                }
            }
            // Goobstation end

            foreach (var message in _controller.History)
            {
                OnMessageAdded(message.Item2);
            }
        }

        private void OnChannelFilter(ChatChannel channel, bool active)
        {
            ResetCoalescenceState();
            Contents.Clear();

            // Goobstation start
            foreach (var child in Contents.Children.ToArray())
            {
                if (child.Name != "_v_scroll")
                {
                    Contents.RemoveChild(child);
                }
            }
            // Goobstation end

            foreach (var message in _controller.History)
            {
                OnMessageAdded(message.Item2);
            }

            if (active)
            {
                _controller.ClearUnfilteredUnreads(channel);
            }
        }
}
