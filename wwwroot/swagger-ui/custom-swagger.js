(() => {
  "use strict";

  const defaultConfig = {
    title: "API Documentation",
    version: "v1",
    oas: "OAS 3.0",
    jsonUrl: "/swagger/v1/swagger.json",
    description: "API documentation.",
    footer: "",
  };

  const storageKey = "custom-swagger-theme";
  let appConfig = { ...defaultConfig };

  const createElement = (tag, className, text) => {
    const element = document.createElement(tag);

    if (className) {
      element.className = className;
    }

    if (text !== undefined && text !== null) {
      element.textContent = text;
    }

    return element;
  };

  const toAbsoluteUrl = (url) => {
    try {
      return new URL(url, window.location.origin).href;
    } catch {
      return url;
    }
  };

  const getSwaggerJsonUrl = () => {
    const input = document.querySelector(".download-url-input");

    if (input && input.value) {
      return input.value;
    }

    return defaultConfig.jsonUrl;
  };

  const normalizeOasVersion = (swaggerJson) => {
    const version = swaggerJson.openapi || swaggerJson.swagger || "";

    if (!version) {
      return "OAS";
    }

    if (swaggerJson.openapi) {
      return `OAS ${version.split(".").slice(0, 2).join(".")}`;
    }

    return `Swagger ${version}`;
  };

  const loadSwaggerInfo = async () => {
    const jsonUrl = getSwaggerJsonUrl();
    const absoluteJsonUrl = toAbsoluteUrl(jsonUrl);

    try {
      const response = await fetch(jsonUrl, { cache: "no-store" });

      if (!response.ok) {
        throw new Error("Swagger JSON failed");
      }

      const swaggerJson = await response.json();
      const info = swaggerJson.info || {};

      appConfig = {
        title: info.title || defaultConfig.title,
        version: info.version || defaultConfig.version,
        oas: normalizeOasVersion(swaggerJson),
        jsonUrl: absoluteJsonUrl,
        description: info.description || defaultConfig.description,
        footer: `© ${new Date().getFullYear()} ${info.title || defaultConfig.title}. All rights reserved.`,
      };
    } catch {
      appConfig = {
        ...defaultConfig,
        jsonUrl: absoluteJsonUrl,
        footer: `© ${new Date().getFullYear()} ${defaultConfig.title}. All rights reserved.`,
      };
    }
  };

  const setTheme = (theme) => {
    document.documentElement.setAttribute("data-swagger-theme", theme);
    localStorage.setItem(storageKey, theme);

    document
      .querySelectorAll(".custom-swagger-theme-button")
      .forEach((button) => {
        button.classList.toggle("is-active", button.dataset.theme === theme);
      });
  };

  const getScrollTarget = (section) => {
    if (section === "home") {
      return document.querySelector(".custom-swagger-hero") || document.body;
    }

    if (section === "endpoints") {
      return (
        document.querySelector(".custom-swagger-search-card") ||
        document.querySelector(".opblock-tag-section")
      );
    }

    if (section === "schemas") {
      return document.querySelector(".swagger-ui .models");
    }

    if (section === "about") {
      return document.querySelector(".custom-swagger-footer");
    }

    return null;
  };

  const scrollToSection = (section) => {
    const target = getScrollTarget(section);

    if (!target) {
      return;
    }

    const headerHeight =
      document.querySelector(".custom-swagger-header")?.offsetHeight || 0;
    const targetTop =
      target.getBoundingClientRect().top + window.scrollY - headerHeight - 18;

    window.scrollTo({
      top: Math.max(targetTop, 0),
      behavior: "smooth",
    });
  };

  const setActiveNav = (section) => {
    document.querySelectorAll(".custom-swagger-nav a").forEach((link) => {
      link.classList.toggle("is-active", link.dataset.section === section);
    });
  };

  const buildHeader = () => {
    if (document.querySelector(".custom-swagger-header")) {
      return;
    }

    const header = createElement("header", "custom-swagger-header");
    const inner = createElement("div", "custom-swagger-header-inner");

    const brand = createElement("div", "custom-swagger-brand");
    const logo = createElement("div", "custom-swagger-logo", "API");
    const title = createElement(
      "div",
      "custom-swagger-brand-title",
      appConfig.title,
    );

    const badges = createElement("div", "custom-swagger-badges");
    badges.append(
      createElement("span", "custom-swagger-badge", appConfig.version),
      createElement("span", "custom-swagger-badge oas", appConfig.oas),
    );

    brand.append(logo, title, badges);

    const toggle = createElement("div", "custom-swagger-theme-toggle");

    const light = createElement(
      "button",
      "custom-swagger-theme-button",
      "Light",
    );
    light.type = "button";
    light.dataset.theme = "light";

    const dark = createElement("button", "custom-swagger-theme-button", "Dark");
    dark.type = "button";
    dark.dataset.theme = "dark";

    light.addEventListener("click", () => setTheme("light"));
    dark.addEventListener("click", () => setTheme("dark"));

    toggle.append(light, dark);

    const nav = createElement("nav", "custom-swagger-nav");

    const navItems = [
      { label: "Home", section: "home" },
      { label: "Endpoints", section: "endpoints", active: true },
      { label: "Schemas", section: "schemas" },
      { label: "About", section: "about" },
    ];

    navItems.forEach((item) => {
      const link = createElement(
        "a",
        item.active ? "is-active" : "",
        item.label,
      );

      link.href = `#${item.section}`;
      link.dataset.section = item.section;

      link.addEventListener("click", (event) => {
        event.preventDefault();
        setActiveNav(item.section);
        scrollToSection(item.section);
      });

      nav.append(link);
    });

    inner.append(brand, toggle, nav);
    header.append(inner);
    document.body.prepend(header);
  };

  const buildHero = () => {
    if (document.querySelector(".custom-swagger-hero")) {
      return;
    }

    const hero = createElement("section", "custom-swagger-hero");
    hero.id = "home";

    const title = createElement(
      "h1",
      "custom-swagger-hero-title",
      appConfig.title,
    );

    const jsonRow = createElement("div", "custom-swagger-json-row");
    jsonRow.append(createElement("span", "", appConfig.jsonUrl));

    const copy = createElement("button", "custom-swagger-copy-json", "⧉");
    copy.type = "button";
    copy.setAttribute("aria-label", "Copy swagger json url");
    copy.title = "Copy full Swagger JSON URL";

    copy.addEventListener("click", async () => {
      try {
        await navigator.clipboard.writeText(appConfig.jsonUrl);
        copy.textContent = "✓";

        window.setTimeout(() => {
          copy.textContent = "⧉";
        }, 900);
      } catch {
        const input = createElement("input");
        input.value = appConfig.jsonUrl;
        document.body.append(input);
        input.select();
        document.execCommand("copy");
        input.remove();

        copy.textContent = "✓";

        window.setTimeout(() => {
          copy.textContent = "⧉";
        }, 900);
      }
    });

    jsonRow.append(copy);

    const description = createElement(
      "p",
      "custom-swagger-description",
      appConfig.description,
    );

    hero.append(title, jsonRow, description);

    const swaggerUi = document.querySelector(".swagger-ui");

    if (swaggerUi) {
      swaggerUi.before(hero);
    }
  };

  const buildSearch = () => {
    if (document.querySelector(".custom-swagger-search-card")) {
      return;
    }

    const firstSection = document.querySelector(".opblock-tag-section");

    if (!firstSection) {
      return;
    }

    const card = createElement("section", "custom-swagger-search-card");
    card.id = "endpoints";

    const inner = createElement("div", "custom-swagger-search-inner");
    const icon = createElement("div", "custom-swagger-search-icon", "⌕");

    const input = createElement("input", "custom-swagger-search-input");
    input.type = "search";
    input.placeholder = "Filter by tag or endpoint";
    input.autocomplete = "off";
    input.spellcheck = false;

    const key = createElement("div", "custom-swagger-search-key", "/");

    let searchTimer = 0;

    input.addEventListener("input", () => {
      clearTimeout(searchTimer);

      searchTimer = window.setTimeout(() => {
        const keyword = input.value.trim().toLowerCase();
        const hasKeyword = keyword.length > 0;

        document.querySelectorAll(".opblock").forEach((opblock) => {
          const method =
            opblock.querySelector(".opblock-summary-method")?.textContent || "";
          const path =
            opblock.querySelector(".opblock-summary-path")?.textContent || "";
          const desc =
            opblock.querySelector(".opblock-summary-description")
              ?.textContent || "";
          const text = `${method} ${path} ${desc}`.toLowerCase();

          opblock.hidden = hasKeyword && !text.includes(keyword);
        });

        document.querySelectorAll(".opblock-tag-section").forEach((section) => {
          const visible = section.querySelector(".opblock:not([hidden])");
          section.hidden = hasKeyword && !visible;
        });
      }, 180);
    });

    document.addEventListener("keydown", (event) => {
      const target = event.target;

      const isTyping =
        target instanceof HTMLInputElement ||
        target instanceof HTMLTextAreaElement ||
        target instanceof HTMLSelectElement;

      if (event.key === "/" && !isTyping) {
        event.preventDefault();
        input.focus();
      }
    });

    inner.append(icon, input, key);
    card.append(inner);
    firstSection.before(card);
  };

  const buildFooter = () => {
    if (document.querySelector(".custom-swagger-footer")) {
      return;
    }

    const footer = createElement(
      "footer",
      "custom-swagger-footer",
      appConfig.footer,
    );
    footer.id = "about";

    document.body.append(footer);
  };

  const markSchemasId = () => {
    const models = document.querySelector(".swagger-ui .models");

    if (models) {
      models.id = "schemas";
    }
  };

  const updateActiveNavOnScroll = () => {
    const sections = ["home", "endpoints", "schemas", "about"];
    const headerHeight =
      document.querySelector(".custom-swagger-header")?.offsetHeight || 0;
    const currentPosition = window.scrollY + headerHeight + 80;

    let activeSection = "home";

    sections.forEach((section) => {
      const target = getScrollTarget(section);

      if (!target) {
        return;
      }

      const top = target.getBoundingClientRect().top + window.scrollY;

      if (currentPosition >= top) {
        activeSection = section;
      }
    });

    setActiveNav(activeSection);
  };

  const initialize = async () => {
    const theme = localStorage.getItem(storageKey) || "light";

    setTheme(theme);

    await loadSwaggerInfo();

    buildHeader();
    buildHero();
    buildSearch();
    buildFooter();
    markSchemasId();

    window.addEventListener(
      "scroll",
      () => {
        window.requestAnimationFrame(updateActiveNavOnScroll);
      },
      { passive: true },
    );
  };

  const waitUntilReady = () => {
    let attempt = 0;
    const maxAttempt = 80;

    const timer = window.setInterval(() => {
      attempt += 1;

      const swaggerUi = document.querySelector(".swagger-ui");
      const firstOperation = document.querySelector(".opblock");

      if (swaggerUi && firstOperation) {
        window.clearInterval(timer);
        initialize();
        return;
      }

      if (attempt >= maxAttempt) {
        window.clearInterval(timer);
      }
    }, 100);
  };

  document.documentElement.setAttribute(
    "data-swagger-theme",
    localStorage.getItem(storageKey) || "light",
  );

  if (document.readyState === "loading") {
    document.addEventListener("DOMContentLoaded", waitUntilReady);
  } else {
    waitUntilReady();
  }
})();
