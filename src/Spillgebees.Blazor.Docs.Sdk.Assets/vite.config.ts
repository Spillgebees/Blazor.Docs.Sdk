import { resolve } from "node:path";
import { copyFileSync, mkdirSync, readdirSync } from "node:fs";
import { defineConfig, type UserConfig, type Plugin } from "vite";

const outDir = resolve(import.meta.dirname!, "../Spillgebees.Blazor.Docs.Sdk/wwwroot");

/** Copies font files from src/fonts/ into the wwwroot/fonts/ output directory. */
function copyFontsPlugin(): Plugin {
  return {
    name: "copy-fonts",
    writeBundle() {
      const fontsIn = resolve(import.meta.dirname!, "src/fonts");
      const fontsOut = resolve(outDir, "fonts");
      mkdirSync(fontsOut, { recursive: true });
      for (const file of readdirSync(fontsIn)) {
        copyFileSync(resolve(fontsIn, file), resolve(fontsOut, file));
      }
    },
  };
}

export default defineConfig(({ mode }) => {
  const isDev = mode === "development";

  return {
    build: {
      lib: {
        entry: resolve(import.meta.dirname!, "src/index.ts"),
        formats: ["es"],
        fileName: () => "Spillgebees.Blazor.Docs.Sdk.lib.module.js",
      },
      outDir,
      emptyOutDir: false,
      sourcemap: isDev,
      minify: !isDev,
      target: "es2022",
      rollupOptions: {
        output: {
          assetFileNames: (assetInfo) => {
            if (assetInfo.names?.some((n) => n.endsWith(".css"))) {
              return "docs-sdk.css";
            }
            return assetInfo.names?.[0] ?? "[name][extname]";
          },
        },
      },
    },
    plugins: [copyFontsPlugin()],
  } satisfies UserConfig;
});
