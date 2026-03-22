import hljs from "highlight.js/lib/core";

// Import only the languages we need (tree-shakeable)
import csharp from "highlight.js/lib/languages/csharp";
import xml from "highlight.js/lib/languages/xml";
import json from "highlight.js/lib/languages/json";
import css from "highlight.js/lib/languages/css";
import javascript from "highlight.js/lib/languages/javascript";
import bash from "highlight.js/lib/languages/bash";

// @ts-ignore - no types available for the Razor plugin
import razor from "highlightjs-cshtml-razor";

// Register languages
hljs.registerLanguage("csharp", csharp);
hljs.registerLanguage("cs", csharp);
hljs.registerLanguage("xml", xml);
hljs.registerLanguage("json", json);
hljs.registerLanguage("css", css);
hljs.registerLanguage("javascript", javascript);
hljs.registerLanguage("js", javascript);
hljs.registerLanguage("bash", bash);
hljs.registerLanguage("shell", bash);
hljs.registerLanguage("cshtml-razor", razor);
hljs.registerLanguage("razor", razor);

// ── Theme ──

const THEME_KEY = "docs-sdk-theme";

function getTheme(): string {
  return localStorage.getItem(THEME_KEY) || "dark";
}

function setTheme(theme: string): void {
  localStorage.setItem(THEME_KEY, theme);
  document.documentElement.classList.remove("theme-light");
  if (theme === "light") {
    document.documentElement.classList.add("theme-light");
  }
}

// ── Clipboard ──

function copyToClipboard(text: string): Promise<void> {
  return navigator.clipboard.writeText(text);
}

// ── Syntax highlighting ──

function highlightElement(element: HTMLElement | null): void {
  if (element) {
    // remove previous highlighting so hljs re-processes the element
    delete element.dataset.highlighted;
    hljs.highlightElement(element);
  }
}

function highlightAll(): void {
  document.querySelectorAll<HTMLElement>("pre code[class*='language-']").forEach((el) => {
    if (!el.dataset.highlighted) {
      hljs.highlightElement(el);
    }
  });
}

// ── Section discovery ──

function getSections(): { title: string; id: string }[] {
  const headings = document.querySelectorAll<HTMLElement>(".doc-content h2[id]");
  return Array.from(headings).map((el) => ({
    title: el.textContent?.trim().replace(/^\/\/\s*/, "") || "",
    id: el.id,
  }));
}

function scrollToElement(id: string): void {
  const el = document.getElementById(id);
  if (el) {
    el.scrollIntoView({ behavior: "smooth", block: "start" });
  }
}

// ── Blazor initializer lifecycle ──

declare global {
  interface Window {
    Spillgebees?: {
      DocsSdk?: {
        getTheme: typeof getTheme;
        setTheme: typeof setTheme;
        copyToClipboard: typeof copyToClipboard;
        highlightElement: typeof highlightElement;
        highlightAll: typeof highlightAll;
        getSections: typeof getSections;
        scrollToElement: typeof scrollToElement;
      };
    };
    hasDocsSdkInitialized?: boolean;
  }
}

function initialize(): void {
  if (window.hasDocsSdkInitialized) {
    return;
  }
  window.hasDocsSdkInitialized = true;

  window.Spillgebees = window.Spillgebees || {};
  window.Spillgebees.DocsSdk = {
    getTheme,
    setTheme,
    copyToClipboard,
    highlightElement,
    highlightAll,
    getSections,
    scrollToElement,
  };

  // apply saved theme on startup
  setTheme(getTheme());
}

export function beforeWebStart(): void {
  initialize();
}

export function afterWebStarted(): void {
  initialize();
}

export function beforeWebAssemblyStart(): void {
  initialize();
}

export function afterWebAssemblyStarted(): void {
  initialize();
}

export function beforeServerStart(): void {
  initialize();
}

export function afterServerStarted(): void {
  initialize();
}
