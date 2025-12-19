(() => {
    const key = "gameshelf_theme_v1"

    const btn = document.getElementById("btnTheme")
    const icon = document.getElementById("themeIcon")
    const label = document.getElementById("themeLabel")

    const prefersLight = () =>
        window.matchMedia && window.matchMedia("(prefers-color-scheme: light)").matches

    const apply = (t) => {
        document.documentElement.dataset.theme = t
        if (icon) icon.textContent = (t === "light") ? "☀️" : "🌙"
        if (label) label.textContent = (t === "light") ? "Light" : "Dark"
        if (btn) btn.setAttribute("aria-pressed", t === "light" ? "true" : "false")
    }

    const init = () => {
        const saved = localStorage.getItem(key)
        const t = saved || (prefersLight() ? "light" : "dark")
        apply(t)
    }

    const toggle = () => {
        const current = document.documentElement.dataset.theme || "dark"
        const next = current === "dark" ? "light" : "dark"
        localStorage.setItem(key, next)
        apply(next)
    }

    if (btn) btn.addEventListener("click", toggle)

    init()
})()
 