﻿namespace Compose3D.GLTypes
{
    using OpenTK.Graphics.OpenGL;
    using System;

	public class GLError : Exception
	{
		public GLError (string msg) : base (msg) { }

		public static GLError GetError ()
		{
			var error = GL.GetError ();
			switch (error)
			{
				case ErrorCode.InvalidEnum:
					return new GLError ("GL_INVALID_ENUM: An unacceptable value has been specified for an enumerated argument");
				case ErrorCode.InvalidFramebufferOperation:
					return new GLError ("GL_INVALID_FRAMEBUFFER_OPERATION: " +
						"The object bound to FRAMEBUFFER_BINDING is not \"framebuffer complete\"");
				case ErrorCode.InvalidOperation:
					return new GLError ("GL_INVALID_OPERATION: The specified operation is not allowed in the current state");
				case ErrorCode.InvalidValue:
					return new GLError ("GL_INVALID_VALUE: A numeric argument is out of range");
				case ErrorCode.NoError:
					return null;
				case ErrorCode.OutOfMemory:
					return new GLError ("GL_OUT_OF_MEMORY: There is not enough memory left to execute the command");
				case ErrorCode.StackOverflow:
					return new GLError ("GL_STACK_OVERFLOW: This command would cause a stack overflow");
				case ErrorCode.StackUnderflow:
					return new GLError ("GL_STACK_UNDERFLOW: This command would cause a stack underflow");
				case ErrorCode.TableTooLarge:
					return new GLError ("GL_TABLE_TOO_LARGE: Specified color lookup table is too large for the implementation");
				case ErrorCode.TextureTooLargeExt:
					return new GLError ("GL_TEXTURE_TOO_LARGE: Specified texture is too large for the implementation");
				default:
					return new GLError ("Unknown OpenGL error");
			}
		}

		public static void CheckError (Action action)
		{
			action ();
			var error = GetError ();
			if (error != null)
				throw error;
		}
	}
}
