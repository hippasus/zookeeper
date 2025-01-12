namespace ApacheZooKeeper.Common;

using System.Runtime.InteropServices;

/**
  * Path related utilities
  */
 public class PathUtils {

     /** validate the provided znode path string
      * @param path znode path string
      * @param isSequential if the path is being created
      * with a sequential flag
      * @throws ArgumentException if the path is invalid
      */
     public static void validatePath(String path, bool isSequential) {
         validatePath(isSequential ? path + "1" : path);
     }

     /**
      * Validate the provided znode path string
      * @param path znode path string
      * @throws ArgumentException if the path is invalid
      */
     public static void validatePath(String path) {
         if (path == null) {
             throw new ArgumentException("Path cannot be null");
         }
         if (path.length() == 0) {
             throw new ArgumentException("Path length must be > 0");
         }
         if (path.charAt(0) != '/') {
             throw new ArgumentException("Path must start with / character");
         }
         if (path.length() == 1) { // done checking - it's the root
             return;
         }
         if (path.charAt(path.length() - 1) == '/') {
             throw new ArgumentException("Path must not end with / character");
         }

         String reason = null;
         char lastc = '/';
         char[] chars = path.toCharArray();
         char c;
         for (int i = 1; i < chars.Length; lastc = chars[i], i++) {
             c = chars[i];

             if (c == 0) {
                 reason = "null character not allowed @" + i;
                 break;
             } else if (c == '/' && lastc == '/') {
                 reason = "empty node name specified @" + i;
                 break;
             } else if (c == '.' && lastc == '.') {
                 if (chars[i - 2] == '/' && ((i + 1 == chars.Length) || chars[i + 1] == '/')) {
                     reason = "relative paths not allowed @" + i;
                     break;
                 }
             } else if (c == '.') {
                 if (chars[i - 1] == '/' && ((i + 1 == chars.Length) || chars[i + 1] == '/')) {
                     reason = "relative paths not allowed @" + i;
                     break;
                 }
             } else if (c > '\u0000' && c <= '\u001f'
                        || c >= '\u007f' && c <= '\u009F'
                        || c >= '\ud800' && c <= '\uf8ff'
                        || c >= '\ufff0' && c <= '\uffff') {
                 reason = "invalid character @" + i;
                 break;
             }
         }

         if (reason != null) {
             throw new ArgumentException("Invalid path string \"" + path + "\" caused by " + reason);
         }
     }

     /**
      * Convert Windows path to Unix
      *
      * @param path
      *            file path
      * @return converted file path
      */
     public static String normalizeFileSystemPath(String path) {
         if (path != null) {
             if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
                 return path.replace('\\', '/');
             }
         }
         return path;
     }

     /**
      * return the top namespace of a znode path
      *
      * @param path znode path string
      *
      * @return the top namespace. If not exist, return null
      */
     public static String getTopNamespace(String path) {
         if (path == null) {
             return null;
         }
         String[] parts = path.split("/");
         return parts.Length > 1 ? parts[1] : null;
     }
 }
