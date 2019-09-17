using System.Collections.Generic;
using System.Numerics;
using Veldrid;
using Veldrid.Sdl2;

namespace ShaderPlayer
{
	public class InputTracker //TODO: use framework input and adapt for Veldrid
	{
		private HashSet<Key> _currentlyPressedKeys = new HashSet<Key>();
		private HashSet<Key> _newKeysThisFrame = new HashSet<Key>();

		private HashSet<MouseButton> _currentlyPressedMouseButtons = new HashSet<MouseButton>();
		private HashSet<MouseButton> _newMouseButtonsThisFrame = new HashSet<MouseButton>();

		public Vector2 MousePosition;
		public Vector2 MouseDelta;

		public bool IsKeyDown(Key key) => _currentlyPressedKeys.Contains(key);

		public bool IsNewKeyDown(Key key) => _newKeysThisFrame.Contains(key);

		public bool GetMouseButton(MouseButton button) => _currentlyPressedMouseButtons.Contains(button);

		public bool GetMouseButtonDown(MouseButton button) => _newMouseButtonsThisFrame.Contains(button);

		public void UpdateFrameInput(InputSnapshot snapshot, Sdl2Window window)
		{
			_newKeysThisFrame.Clear();
			_newMouseButtonsThisFrame.Clear();

			MousePosition = snapshot.MousePosition;
			MouseDelta = window.MouseDelta;
			for (int i = 0; i < snapshot.KeyEvents.Count; i++)
			{
				KeyEvent ke = snapshot.KeyEvents[i];
				if (ke.Down)
				{
					KeyDown(ke.Key);
				}
				else
				{
					KeyUp(ke.Key);
				}
			}
			for (int i = 0; i < snapshot.MouseEvents.Count; i++)
			{
				MouseEvent me = snapshot.MouseEvents[i];
				if (me.Down)
				{
					MouseDown(me.MouseButton);
				}
				else
				{
					MouseUp(me.MouseButton);
				}
			}
		}

		private void MouseUp(MouseButton mouseButton)
		{
			_currentlyPressedMouseButtons.Remove(mouseButton);
			_newMouseButtonsThisFrame.Remove(mouseButton);
		}

		private void MouseDown(MouseButton mouseButton)
		{
			if (_currentlyPressedMouseButtons.Add(mouseButton))
			{
				_newMouseButtonsThisFrame.Add(mouseButton);
			}
		}

		private void KeyUp(Key key)
		{
			_currentlyPressedKeys.Remove(key);
			_newKeysThisFrame.Remove(key);
		}

		private void KeyDown(Key key)
		{
			if (_currentlyPressedKeys.Add(key))
			{
				_newKeysThisFrame.Add(key);
			}
		}
	}
}
