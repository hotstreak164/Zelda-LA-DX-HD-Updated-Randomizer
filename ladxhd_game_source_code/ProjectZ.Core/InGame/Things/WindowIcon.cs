using System;
using System.IO;
using System.Runtime.InteropServices;

internal static class WindowIcon
{
    private const string SDL2_LIB = "SDL2";

    [DllImport(SDL2_LIB, CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr SDL_RWFromMem(IntPtr mem, int size);

    [DllImport(SDL2_LIB, CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr SDL_LoadBMP_RW(IntPtr src, int freesrc);

    [DllImport(SDL2_LIB, CallingConvention = CallingConvention.Cdecl)]
    private static extern void SDL_SetWindowIcon(IntPtr window, IntPtr icon);

    [DllImport(SDL2_LIB, CallingConvention = CallingConvention.Cdecl)]
    private static extern void SDL_FreeSurface(IntPtr surface);

    public static bool SetFromStream(IntPtr sdlWindowHandle, Stream bmpStream)
    {
        if (sdlWindowHandle == IntPtr.Zero || bmpStream == null)
            return false;

        using var ms = new MemoryStream();
        bmpStream.CopyTo(ms);
        var bytes = ms.ToArray();
        if (bytes.Length == 0)
            return false;

        var handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
        try
        {
            // Create an SDL RWops from our pinned buffer
            IntPtr rw = SDL_RWFromMem(handle.AddrOfPinnedObject(), bytes.Length);
            if (rw == IntPtr.Zero)
                return false;

            // Load a surface from the RWops. freesrc=1 => SDL will free RWops.
            IntPtr surface = SDL_LoadBMP_RW(rw, 1);
            if (surface == IntPtr.Zero)
                return false;

            try
            {
                SDL_SetWindowIcon(sdlWindowHandle, surface);
                return true;
            }
            finally
            {
                SDL_FreeSurface(surface);
            }
        }
        finally
        {
            handle.Free();
        }
    }
}