import { resolve } from "node:path";
import { defineConfig, type UserConfig } from "vite";

export default defineConfig(({ mode }) => {
  const isDev = mode === "development";

  return {
    build: {
      lib: {
        entry: resolve(import.meta.dirname!, "src/index.ts"),
        formats: ["es"],
        fileName: () => "Spillgebees.Blazor.Docs.Sdk.lib.module.js",
      },
      outDir: resolve(import.meta.dirname!, "../Spillgebees.Blazor.Docs.Sdk/wwwroot"),
      emptyOutDir: false,
      sourcemap: isDev,
      minify: !isDev,
      target: "es2022",
    },
  } satisfies UserConfig;
});
