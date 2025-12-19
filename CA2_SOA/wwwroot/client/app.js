(() => {
    const $ = (id) => document.getElementById(id);

    // Top
    const apiBaseEl = $("apiBase");
    const tokenEl = $("token");
    const rawgKeyEl = $("rawgKey");
    const btnSave = $("btnSave");
    const btnClear = $("btnClear");

    // Header actions
    const btnExportBackup = $("btnExportBackup");

    // Store buttons
    const btnRefresh2 = $("btnRefresh2");
    const btnClearList = $("btnClearList");

    // Store output
    const outList = $("outList");

    // Create
    const gTitle = $("gTitle");
    const gPlatform = $("gPlatform");
    const gDesc = $("gDesc");
    const gYear = $("gYear");
    const gGenreId = $("gGenreId");
    const gImageUrl = $("gImageUrl");
    const btnCreate = $("btnCreate");
    const btnAutofill = $("btnAutofill");
    const autofillHint = $("autofillHint");
    const createErrors = $("createErrors");
    const btnClearCreate = $("btnClearCreate");
    const imgPreviewWrap = $("imgPreviewWrap");
    const imgPreview = $("imgPreview");
    const imgPreviewMsg = $("imgPreviewMsg");
    const outCreate = $("outCreate");

    // List
    const gamesGrid = $("gamesGrid");

    // Filters (now inside Store)
    const fSearch = $("fSearch");
    const fGenre = $("fGenre");
    const fPlatform = $("fPlatform");
    const fHasImage = $("fHasImage");
    const btnClearFilters = $("btnClearFilters");
    const btnDoSearch = $("btnDoSearch");
    const sortBy = $("sortBy");

    // Auth
    const authUi = $("authUi");
    const signedInUi = $("signedInUi");
    const signedInText = $("signedInText");
    const btnLogout = $("btnLogout");
    const btnMeSignedIn = $("btnMeSignedIn");

    const segLogin = $("segLogin");
    const segRegister = $("segRegister");
    const loginPanel = $("loginPanel");
    const registerPanel = $("registerPanel");
    const authErrors = $("authErrors");
    const outAuth = $("outAuth");
    const outReg = $("outReg");
    const btnClearAuth = $("btnClearAuth");

    const loginUser = $("loginUser");
    const loginPass = $("loginPass");
    const btnLogin = $("btnLogin");
    const btnMe = $("btnMe");

    const regUser = $("regUser");
    const regEmail = $("regEmail");
    const regPass = $("regPass");
    const btnRegister = $("btnRegister");

    // Details modal (optional)
    const detailsOverlay = $("detailsOverlay");
    const btnCloseDetails = $("btnCloseDetails");
    const detailsHero = $("detailsHero");
    const detailsTitle = $("detailsTitle");
    const detailsMeta = $("detailsMeta");
    const detailsDesc = $("detailsDesc");
    const detailsJson = $("detailsJson");
    const btnCopyDetails = $("btnCopyDetails");

    // State
    let cachedGenres = [];
    let lastGames = [];

    // Storage
    const storageKey = "gameshelf_client_v1";

    function loadSaved() {
        try {
            const raw = localStorage.getItem(storageKey);
            if (!raw) return;
            const s = JSON.parse(raw);
            if (apiBaseEl) apiBaseEl.value = s.apiBase ?? "";
            if (tokenEl) tokenEl.value = s.token ?? "";
            if (rawgKeyEl) rawgKeyEl.value = s.rawgKey ?? "";
        } catch {}
    }

    function save() {
        const s = {
            apiBase: (apiBaseEl?.value || "").trim(),
            token: (tokenEl?.value || "").trim(),
            rawgKey: (rawgKeyEl?.value || "").trim(),
        };
        localStorage.setItem(storageKey, JSON.stringify(s));
    }

    function clearSaved() {
        localStorage.removeItem(storageKey);
        if (apiBaseEl) apiBaseEl.value = "";
        if (tokenEl) tokenEl.value = "";
        if (rawgKeyEl) rawgKeyEl.value = "";
    }

    function baseUrl() {
        const v = (apiBaseEl?.value || "").trim();
        return v ? v.replace(/\/+$/, "") : "";
    }

    function url(path) {
        const b = baseUrl();
        return b ? (b + path) : path;
    }

    function token() {
        return (tokenEl?.value || "").trim();
    }

    function rawgKey() {
        const fromBox = (rawgKeyEl?.value || "").trim();
        const fromConfig = (window.__RAWG_KEY || "").trim();
        return fromBox || fromConfig || "";
    }

    function headers(json = true) {
        const h = {};
        if (json) h["Content-Type"] = "application/json";
        const t = token();
        if (t) h["Authorization"] = `Bearer ${t}`;
        return h;
    }

    function showErrors(el, lines) {
        if (!el) return;
        if (!lines || lines.length === 0) {
            el.style.display = "none";
            el.textContent = "";
            return;
        }
        el.style.display = "block";
        el.textContent = lines.join("\n");
    }

    function safeJsonParse(text) {
        try { return JSON.parse(text); } catch { return text; }
    }

    async function readBody(resp) {
        const ct = resp.headers.get("content-type") || "";
        const text = await resp.text();
        if (ct.includes("application/json")) return safeJsonParse(text);
        return text;
    }

    function normalizePlatform(s) {
        const v = (s ?? "").trim();
        if (!v) return v;
        const low = v.toLowerCase();
        if (low === "pc" || low.includes("windows")) return "PC";
        if (low.includes("playstation") || low === "ps5" || low === "ps4" || low === "ps") return "PlayStation";
        if (low.includes("xbox")) return "Xbox";
        if (low.includes("switch") || low.includes("nintendo")) return "Switch";
        return v;
    }

    function isDirectImageUrl(u) {
        if (!u) return false;
        const s = u.trim();
        if (/^data:image\//i.test(s)) return true;
        if (/^https?:\/\//i.test(s)) return true;
        return /\.(png|jpg|jpeg|webp|gif)(\?.*)?$/i.test(s);
    }

    function setPreview(urlMaybe) {
        const u = (urlMaybe || "").trim();
        if (imgPreviewWrap) imgPreviewWrap.style.display = "block";

        if (!u) {
            if (imgPreview) imgPreview.src = "";
            if (imgPreview) imgPreview.style.display = "none";
            if (imgPreviewMsg) imgPreviewMsg.textContent = "No image";
            return;
        }

        if (imgPreviewMsg) imgPreviewMsg.textContent = "";
        if (imgPreview) imgPreview.style.display = "block";
        if (imgPreview) imgPreview.src = u;
        if (imgPreview) {
            imgPreview.onerror = () => {
                imgPreview.style.display = "none";
                if (imgPreviewMsg) imgPreviewMsg.textContent = "Image URL didn’t load. RAWG usually fixes this.";
            };
        }
    }

    function truncate(s, n) {
        const t = (s || "").trim();
        if (!t) return "";
        return t.length > n ? (t.slice(0, n - 1) + "…") : t;
    }

    function resetCreateForm() {
        if (gTitle) gTitle.value = "";
        if (gPlatform) gPlatform.value = "";
        if (gDesc) gDesc.value = "";
        if (gYear) gYear.value = "";
        if (gGenreId) gGenreId.value = "";
        if (gImageUrl) gImageUrl.value = "";
        if (autofillHint) autofillHint.textContent = "";
        showErrors(createErrors, []);
        setPreview("");
        if (outCreate) outCreate.textContent = "{}";
    }

    function getImageUrlFromGame(g) {
        return (
            (g?.imageUrl ?? "") ||
            (g?.imageURL ?? "") ||
            (g?.coverUrl ?? "") ||
            (g?.coverURL ?? "") ||
            (g?.thumbnailUrl ?? "") ||
            (g?.thumbnailURL ?? "")
        ).trim();
    }

    async function loadGenres() {
        cachedGenres = [];
        if (fGenre) fGenre.innerHTML = `<option value="">All genres</option>`;
        try {
            const resp = await fetch(url("/api/genres"), { headers: headers(false) });
            if (!resp.ok) return;
            const data = await resp.json();
            if (!Array.isArray(data)) return;
            cachedGenres = data;

            if (!fGenre) return;
            for (const g of cachedGenres) {
                const opt = document.createElement("option");
                opt.value = String(g.id ?? "");
                opt.textContent = g.name ?? `Genre ${g.id}`;
                fGenre.appendChild(opt);
            }
        } catch {}
    }

    function inferGenreIdFromName(name) {
        if (!cachedGenres.length) return "";
        const n = (name || "").toLowerCase();

        let want = "Action";
        if (n.includes("role") || n.includes("rpg")) want = "RPG";
        else if (n.includes("shoot")) want = "Shooter";
        else if (n.includes("platform")) want = "Platformer";
        else if (n.includes("puzzle")) want = "Puzzle";
        else if (n.includes("action")) want = "Action";

        const match = cachedGenres.find(g => (g.name || "").toLowerCase() === want.toLowerCase());
        return match ? String(match.id) : "";
    }

    async function fetchRawgBestMatch(title) {
        const key = rawgKey();
        const t = (title || "").trim();
        if (!key || !t) return null;

        const searchUrl =
            `https://api.rawg.io/api/games?search=${encodeURIComponent(t)}&page_size=5&key=${encodeURIComponent(key)}`;

        const sResp = await fetch(searchUrl);
        if (!sResp.ok) return null;
        const sJson = await sResp.json();
        const first = sJson?.results?.[0];
        if (!first?.id) return null;

        const detUrl = `https://api.rawg.io/api/games/${first.id}?key=${encodeURIComponent(key)}`;
        const dResp = await fetch(detUrl);
        if (!dResp.ok) return null;
        const dJson = await dResp.json();

        const released = dJson?.released || first.released || "";
        const year = released ? String(new Date(released).getFullYear()) : "";

        const img = dJson?.background_image || first.background_image || "";
        const desc = (dJson?.description_raw || "").trim();

        const platformName =
            dJson?.platforms?.[0]?.platform?.name ||
            first?.platforms?.[0]?.platform?.name ||
            "";

        const genreName =
            dJson?.genres?.[0]?.name ||
            first?.genres?.[0]?.name ||
            "";

        return {
            description: desc,
            imageUrl: img,
            releaseYear: year,
            platform: normalizePlatform(platformName),
            genreName: genreName
        };
    }

    async function fetchWikiDescription(title) {
        const t = (title || "").trim();
        if (!t) return null;

        const endpoint =
            `https://en.wikipedia.org/api/rest_v1/page/summary/${encodeURIComponent(t)}`;

        const resp = await fetch(endpoint);
        if (!resp.ok) return null;

        const j = await resp.json();
        const desc = (j?.extract || "").trim();
        return desc ? { description: desc } : null;
    }

    async function autofill() {
        showErrors(createErrors, []);
        if (autofillHint) autofillHint.textContent = "Looking up…";

        const title = gTitle?.value.trim() || "";
        if (!title) {
            if (autofillHint) autofillHint.textContent = "";
            showErrors(createErrors, ["Title is required to auto-fill."]);
            return;
        }

        let data = null;

        try {
            data = await fetchRawgBestMatch(title);
        } catch {
            data = null;
        }

        if (!data && !(gDesc?.value.trim() || "")) {
            try {
                const wiki = await fetchWikiDescription(title);
                if (wiki?.description) data = { description: wiki.description };
            } catch {}
        }

        if (!data) {
            if (autofillHint) autofillHint.textContent = "No match found.";
            return;
        }

        if (data.description && !(gDesc?.value.trim() || "")) gDesc.value = truncate(data.description, 500);
        if (data.releaseYear && !(gYear?.value.trim() || "")) gYear.value = data.releaseYear;
        if (data.platform && !(gPlatform?.value.trim() || "")) gPlatform.value = data.platform;

        if (data.imageUrl && !(gImageUrl?.value.trim() || "")) gImageUrl.value = data.imageUrl;

        if (data.genreName && (gGenreId && !String(gGenreId.value || "").trim())) {
            const mapped = inferGenreIdFromName(data.genreName);
            if (mapped) gGenreId.value = mapped;
        }

        setPreview(gImageUrl?.value || "");

        if (autofillHint) {
            if (data.imageUrl) autofillHint.textContent = "Auto-filled from RAWG (image + info).";
            else autofillHint.textContent = "Filled description only.";
        }
    }

    function validateCreatePayload(p) {
        const errs = [];
        if (!p.title || !p.title.trim()) errs.push("Title is required.");
        if (!p.platform || !p.platform.trim()) errs.push("Platform is required.");

        if (p.releaseYear != null && p.releaseYear !== "") {
            const y = Number(p.releaseYear);
            if (!Number.isInteger(y)) errs.push("Release year must be a whole number.");
            if (y < 1950 || y > 2100) errs.push("Release year must be between 1950 and 2100.");
        }

        if (p.genreId != null && p.genreId !== "") {
            const gid = Number(p.genreId);
            if (!Number.isInteger(gid) || gid < 1) errs.push("GenreId must be a positive number.");
            if (cachedGenres.length) {
                const ok = cachedGenres.some(g => Number(g.id) === gid);
                if (!ok) errs.push(`GenreId ${gid} does not exist. Pick a real one from /api/genres.`);
            }
        }

        return errs;
    }

    async function enrichFromRawgOnCreateIfNeeded() {
        const title = gTitle?.value.trim() || "";
        if (!title) return;

        const needImage = !(gImageUrl?.value.trim() || "");
        const needDesc = !(gDesc?.value.trim() || "");
        const needYear = !(gYear?.value.trim() || "");
        const needPlatform = !(gPlatform?.value.trim() || "");
        const needGenre = !String(gGenreId?.value || "").trim();

        if (!needImage && !needDesc && !needYear && !needPlatform && !needGenre) return;

        let data = null;
        try {
            data = await fetchRawgBestMatch(title);
        } catch {
            data = null;
        }

        if (data) {
            if (needDesc && data.description) gDesc.value = truncate(data.description, 500);
            if (needYear && data.releaseYear) gYear.value = data.releaseYear;
            if (needPlatform && data.platform) gPlatform.value = data.platform;
            if (needImage && data.imageUrl) gImageUrl.value = data.imageUrl;

            if (needGenre && data.genreName && gGenreId) {
                const mapped = inferGenreIdFromName(data.genreName);
                if (mapped) gGenreId.value = mapped;
            }

            setPreview(gImageUrl?.value || "");
            return;
        }

        if (needDesc) {
            try {
                const wiki = await fetchWikiDescription(title);
                if (wiki?.description) gDesc.value = truncate(wiki.description, 500);
            } catch {}
        }
    }

    async function createGame() {
        showErrors(createErrors, []);
        if (autofillHint) autofillHint.textContent = "";

        await enrichFromRawgOnCreateIfNeeded();

        const payload = {
            title: gTitle?.value.trim() || "",
            platform: normalizePlatform(gPlatform?.value || ""),
            description: (gDesc?.value.trim() || "") || null,
            releaseYear: (gYear?.value.trim() || "") ? Number(gYear.value.trim()) : null,
            genreId: String(gGenreId?.value || "").trim() ? Number(gGenreId.value) : null,
            imageUrl: (gImageUrl?.value.trim() || "") || null,
        };

        const errs = validateCreatePayload(payload);
        if (errs.length) {
            showErrors(createErrors, errs);
            return;
        }

        try {
            const resp = await fetch(url("/api/games"), {
                method: "POST",
                headers: headers(true),
                body: JSON.stringify(payload),
            });

            const body = await readBody(resp);
            if (outCreate) outCreate.textContent = JSON.stringify(body, null, 2);

            if (!resp.ok) {
                showErrors(createErrors, [typeof body === "string" ? body : JSON.stringify(body)]);
                return;
            }

            const newGame =
                (body && typeof body === "object" && !Array.isArray(body))
                    ? { ...payload, ...body }
                    : { ...payload };

            if (!newGame.imageUrl && payload.imageUrl) newGame.imageUrl = payload.imageUrl;

            if (newGame.id != null) {
                lastGames = [newGame, ...lastGames.filter(g => String(g.id) !== String(newGame.id))];
            } else {
                lastGames = [newGame, ...lastGames];
            }

            buildPlatformOptions(lastGames);
            applyFilters();

            await refreshGames();

            resetCreateForm();
        } catch (e) {
            showErrors(createErrors, [String(e)]);
        }
    }

    async function deleteGame(id) {
        if (!confirm(`Delete game #${id}?`)) return;

        try {
            const resp = await fetch(url(`/api/games/${id}`), {
                method: "DELETE",
                headers: headers(false),
            });

            if (resp.status === 204) {
                await refreshGames();
                return;
            }

            const body = await readBody(resp);
            alert(typeof body === "string" ? body : JSON.stringify(body));
            if (resp.ok) await refreshGames();
        } catch (e) {
            alert(String(e));
        }
    }

    function buildPlatformOptions(games) {
        if (!fPlatform) return;

        const current = fPlatform.value;
        const values = Array.from(new Set((games || [])
            .map(g => (g.platform || "").trim())
            .filter(Boolean)))
            .sort((a, b) => a.localeCompare(b));

        fPlatform.innerHTML = `<option value="">All platforms</option>`;
        for (const v of values) {
            const opt = document.createElement("option");
            opt.value = v;
            opt.textContent = v;
            fPlatform.appendChild(opt);
        }
        if (values.includes(current)) fPlatform.value = current;
    }

    function sortGames(games) {
        const mode = (sortBy?.value || "").trim();

        const yearNum = (g) => {
            const v = Number(g?.releaseYear ?? 0);
            return Number.isFinite(v) ? v : 0;
        };

        const titleKey = (g) => String(g?.title || "").toLowerCase();

        const arr = [...(games || [])];

        if (mode === "year_asc") {
            arr.sort((a, b) => yearNum(a) - yearNum(b) || titleKey(a).localeCompare(titleKey(b)));
        } else if (mode === "title_asc") {
            arr.sort((a, b) => titleKey(a).localeCompare(titleKey(b)));
        } else if (mode === "title_desc") {
            arr.sort((a, b) => titleKey(b).localeCompare(titleKey(a)));
        } else {
            arr.sort((a, b) => yearNum(b) - yearNum(a) || titleKey(a).localeCompare(titleKey(b)));
        }

        return arr;
    }

    function applyFilters() {
        const q = (fSearch?.value || "").trim().toLowerCase();
        const gid = (fGenre?.value || "").trim();
        const plat = (fPlatform?.value || "").trim();
        const mustHaveImg = !!fHasImage?.checked;

        let games = [...lastGames];

        if (q) games = games.filter(g => ((g.title || "").toLowerCase().includes(q)));
        if (gid) games = games.filter(g => String(g.genreId ?? "") === gid);
        if (plat) games = games.filter(g => (g.platform || "") === plat);
        if (mustHaveImg) games = games.filter(g => !!getImageUrlFromGame(g));

        renderGames(sortGames(games));
    }

    async function runSearch() {
        if (!lastGames.length) {
            await refreshGames();
            return;
        }
        applyFilters();
    }

    function openDetails(game) {
        if (!detailsOverlay) return;

        const img = getImageUrlFromGame(game);
        if (detailsHero) {
            detailsHero.style.background = img
                ? `linear-gradient(90deg, rgba(0,0,0,.55), rgba(0,0,0,.1)), url("${img}") center/cover no-repeat`
                : `linear-gradient(90deg, rgba(0,0,0,.55), rgba(0,0,0,.1))`;
        }

        if (detailsTitle) detailsTitle.textContent = game?.title ?? "";
        const metaBits = [
            `#${game?.id ?? ""}`,
            game?.platform || "",
            game?.releaseYear ? String(game.releaseYear) : "",
            game?.genreName || (game?.genreId ? `Genre ${game.genreId}` : "")
        ].filter(Boolean);

        if (detailsMeta) detailsMeta.textContent = metaBits.join(" • ");
        if (detailsDesc) detailsDesc.textContent = (game?.description || "").trim() || "No description.";
        if (detailsJson) detailsJson.textContent = JSON.stringify(game, null, 2);

        detailsOverlay.classList.remove("hidden");
        detailsOverlay.setAttribute("aria-hidden", "false");
    }

    function closeDetails() {
        if (!detailsOverlay) return;
        detailsOverlay.classList.add("hidden");
        detailsOverlay.setAttribute("aria-hidden", "true");
    }

    function renderGames(games) {
        if (!gamesGrid) return;
        gamesGrid.innerHTML = "";

        if (!games || games.length === 0) {
            gamesGrid.innerHTML = `<div class="muted small" style="padding:8px 2px;">No games found.</div>`;
            return;
        }

        for (const g of games) {
            const card = document.createElement("div");
            card.className = "game-card";
            card.tabIndex = 0;

            const cover = document.createElement("div");
            cover.className = "game-cover";

            const imgUrl = getImageUrlFromGame(g);

            if (imgUrl && isDirectImageUrl(imgUrl)) {
                const img = document.createElement("img");
                img.src = imgUrl;
                img.alt = g.title || "cover";
                img.loading = "lazy";
                img.onerror = () => { cover.innerHTML = `<div class="muted small">No image</div>`; };
                cover.appendChild(img);
            } else {
                cover.innerHTML = `<div class="muted small">No image</div>`;
            }

            const meta = document.createElement("div");
            meta.className = "game-meta";

            const title = document.createElement("div");
            title.className = "game-title";
            title.textContent = g.title ?? `(game ${g.id})`;

            const sub = document.createElement("div");
            sub.className = "game-sub";

            const t1 = document.createElement("span");
            t1.className = "tag";
            t1.textContent = `#${g.id ?? "new"}`;

            const t2 = document.createElement("span");
            t2.className = "tag";
            t2.textContent = g.platform ? g.platform : "Platform?";

            const t3 = document.createElement("span");
            t3.className = "tag";
            t3.textContent = g.releaseYear ? String(g.releaseYear) : "Year?";

            const t4 = document.createElement("span");
            t4.className = "tag";
            t4.textContent = g.genreName ? g.genreName : (g.genreId ? `Genre ${g.genreId}` : "No genre");

            sub.appendChild(t1);
            sub.appendChild(t2);
            sub.appendChild(t3);
            sub.appendChild(t4);

            meta.appendChild(title);
            meta.appendChild(sub);

            const actions = document.createElement("div");
            actions.className = "game-actions";

            const del = document.createElement("button");
            del.className = "btn tiny danger";
            del.textContent = "Delete";
            del.onclick = (e) => {
                e.stopPropagation();
                if (g.id != null) deleteGame(g.id);
            };

            actions.appendChild(del);

            card.appendChild(cover);
            card.appendChild(meta);
            card.appendChild(actions);

            card.onclick = () => openDetails(g);
            card.onkeydown = (e) => {
                if (e.key === "Enter" || e.key === " ") openDetails(g);
            };

            gamesGrid.appendChild(card);
        }
    }

    async function refreshGames() {
        try {
            const prevImg = new Map(
                (lastGames || [])
                    .filter(g => g?.id != null)
                    .map(g => [String(g.id), getImageUrlFromGame(g)])
                    .filter(([, v]) => !!v)
            );

            const resp = await fetch(url("/api/games"), { headers: headers(false) });
            const body = await readBody(resp);

            if (outList) outList.textContent = JSON.stringify(body, null, 2);

            if (!resp.ok) {
                lastGames = [];
                applyFilters();
                return;
            }

            lastGames = Array.isArray(body) ? body : [];

            for (const g of lastGames) {
                if (g?.id == null) continue;
                const id = String(g.id);
                const have = getImageUrlFromGame(g);
                if (!have && prevImg.has(id)) {
                    g.imageUrl = prevImg.get(id);
                }
            }

            buildPlatformOptions(lastGames);
            applyFilters();
        } catch (e) {
            showErrors(createErrors, [String(e)]);
        }
    }

    // Auth
    function setSignedInUI(isSignedIn, who = "") {
        if (authUi) authUi.style.display = isSignedIn ? "none" : "block";
        if (signedInUi) signedInUi.style.display = isSignedIn ? "block" : "none";
        if (signedInText) signedInText.textContent = who ? `Signed in as ${who}` : "Signed in";
    }

    function setAuthMode(mode) {
        if (!segLogin || !segRegister || !loginPanel || !registerPanel) return;

        if (mode === "login") {
            segLogin.classList.add("active");
            segRegister.classList.remove("active");
            loginPanel.style.display = "block";
            registerPanel.style.display = "none";
        } else {
            segRegister.classList.add("active");
            segLogin.classList.remove("active");
            loginPanel.style.display = "none";
            registerPanel.style.display = "block";
        }
        showErrors(authErrors, []);
    }

    async function doRegister() {
        showErrors(authErrors, []);
        const u = regUser?.value.trim() || "";
        const e = regEmail?.value.trim() || "";
        const p = regPass?.value || "";

        const errs = [];
        if (!u) errs.push("Username is required.");
        if (!e) errs.push("Email is required.");
        if (!p) errs.push("Password is required.");
        if (errs.length) { showErrors(authErrors, errs); return; }

        try {
            const resp = await fetch(url("/api/auth/register"), {
                method: "POST",
                headers: headers(true),
                body: JSON.stringify({ userName: u, email: e, password: p }),
            });

            const body = await readBody(resp);
            if (outReg) outReg.textContent = JSON.stringify(body, null, 2);

            if (resp.ok) setAuthMode("login");
        } catch (e2) {
            if (outReg) outReg.textContent = String(e2);
        }
    }

    async function doLogin() {
        showErrors(authErrors, []);
        const u = loginUser?.value.trim() || "";
        const p = loginPass?.value || "";

        const errs = [];
        if (!u) errs.push("Username is required.");
        if (!p) errs.push("Password is required.");
        if (errs.length) { showErrors(authErrors, errs); return; }

        try {
            const resp = await fetch(url("/api/auth/login"), {
                method: "POST",
                headers: headers(true),
                body: JSON.stringify({ userNameOrEmail: u, password: p }),
            });

            const body = await readBody(resp);
            if (outAuth) outAuth.textContent = JSON.stringify(body, null, 2);

            if (resp.ok) {
                const t = body?.token || body?.accessToken || body?.jwt || "";
                if (t && tokenEl) {
                    tokenEl.value = t;
                    save();
                    await doMe(true);
                }
            }
        } catch (e2) {
            if (outAuth) outAuth.textContent = String(e2);
        }
    }

    async function doMe(silent = false) {
        if (!silent) showErrors(authErrors, []);
        try {
            const resp = await fetch(url("/api/auth/me"), { headers: headers(false) });
            const body = await readBody(resp);

            if (!silent && outAuth) outAuth.textContent = JSON.stringify(body, null, 2);

            if (resp.ok) {
                const who = body?.userName || body?.username || body?.email || "";
                setSignedInUI(true, who);
            } else {
                setSignedInUI(false, "");
            }
        } catch (e) {
            if (!silent && outAuth) outAuth.textContent = String(e);
            setSignedInUI(false, "");
        }
    }

    function logout() {
        if (tokenEl) tokenEl.value = "";
        save();
        setSignedInUI(false, "");
        setAuthMode("login");
    }

    async function exportBackup() {
        const stamp = new Date().toISOString().replaceAll(":", "-");
        const out = { exportedAt: new Date().toISOString(), apiBase: baseUrl(), data: {} };

        const endpoints = [
            ["genres", "/api/genres"],
            ["games", "/api/games"],
            ["me", "/api/auth/me"],
            ["libraryEntries", "/api/libraryentries"],
            ["reviews", "/api/reviews"],
        ];

        for (const [name, path] of endpoints) {
            try {
                const resp = await fetch(url(path), { headers: headers(false) });
                out.data[name] = resp.ok ? await resp.json() : { status: resp.status };
            } catch (e) {
                out.data[name] = { error: String(e) };
            }
        }

        const blob = new Blob([JSON.stringify(out, null, 2)], { type: "application/json" });
        const a = document.createElement("a");
        a.href = URL.createObjectURL(blob);
        a.download = `gameshelf-backup-${stamp}.json`;
        document.body.appendChild(a);
        a.click();
        a.remove();
        URL.revokeObjectURL(a.href);
    }

    // Wire up
    if (btnSave) btnSave.onclick = async () => { save(); await loadGenres(); await refreshGames(); };
    if (btnClear) btnClear.onclick = () => { clearSaved(); };

    if (btnCreate) btnCreate.onclick = createGame;
    if (btnAutofill) btnAutofill.onclick = autofill;

    if (btnRefresh2) btnRefresh2.onclick = refreshGames;

    if (btnClearList) btnClearList.onclick = () => {
        if (gamesGrid) gamesGrid.innerHTML = "";
        if (outList) outList.textContent = "[]";
    };

    if (btnClearCreate) btnClearCreate.onclick = () => { resetCreateForm(); };

    if (gImageUrl) gImageUrl.addEventListener("input", () => setPreview(gImageUrl.value));

    // Filters: button + Enter
    if (btnDoSearch) btnDoSearch.onclick = runSearch;
    if (fSearch) {
        fSearch.addEventListener("keydown", (e) => {
            if (e.key === "Enter") runSearch();
        });
    }

    // Sorting 
    if (sortBy) sortBy.addEventListener("change", applyFilters);

    // filter
    if (fSearch) fSearch.addEventListener("input", applyFilters);
    if (fGenre) fGenre.addEventListener("change", applyFilters);
    if (fPlatform) fPlatform.addEventListener("change", applyFilters);
    if (fHasImage) fHasImage.addEventListener("change", applyFilters);

    if (btnClearFilters) btnClearFilters.onclick = () => {
        if (fSearch) fSearch.value = "";
        if (fGenre) fGenre.value = "";
        if (fPlatform) fPlatform.value = "";
        if (fHasImage) fHasImage.checked = false;
        if (sortBy) sortBy.value = "year_desc";
        applyFilters();
    };

    if (segLogin) segLogin.onclick = () => setAuthMode("login");
    if (segRegister) segRegister.onclick = () => setAuthMode("register");

    if (btnRegister) btnRegister.onclick = doRegister;
    if (btnLogin) btnLogin.onclick = doLogin;
    if (btnMe) btnMe.onclick = () => doMe(false);

    if (btnMeSignedIn) btnMeSignedIn.onclick = () => doMe(false);
    if (btnLogout) btnLogout.onclick = logout;

    if (btnClearAuth) btnClearAuth.onclick = () => {
        if (outAuth) outAuth.textContent = "{}";
        if (outReg) outReg.textContent = "{}";
        showErrors(authErrors, []);
    };

    if (btnExportBackup) btnExportBackup.onclick = exportBackup;

    if (btnCloseDetails) btnCloseDetails.onclick = closeDetails;
    if (detailsOverlay) {
        detailsOverlay.onclick = (e) => {
            if (e.target === detailsOverlay) closeDetails();
        };
    }
    if (btnCopyDetails) {
        btnCopyDetails.onclick = async () => {
            try {
                await navigator.clipboard.writeText(detailsJson?.textContent || "");
            } catch {}
        };
    }
    
    loadSaved();

    if (rawgKeyEl && !rawgKeyEl.value.trim() && (window.__RAWG_KEY || "").trim()) {
        rawgKeyEl.value = (window.__RAWG_KEY || "").trim();
    }

    setPreview("");
    setAuthMode("login");
    setSignedInUI(false, "");

    loadGenres().finally(async () => {
        await refreshGames();
        if (token()) await doMe(true);
    });
})();
