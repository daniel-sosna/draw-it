import globals from "globals";
import pluginReact from "eslint-plugin-react";
import reactHooks from 'eslint-plugin-react-hooks';
import reactRefresh from 'eslint-plugin-react-refresh';
import { defineConfig } from "eslint/config";

export default defineConfig([
    { files: ["**/*.{js,mjs,cjs,jsx}"], languageOptions: { globals: globals.browser } },
    pluginReact.configs.flat.recommended,
    pluginReact.configs.flat['jsx-runtime'], // For React 17+
    {
        settings: { react: { version: "detect", }, },
        plugins: { "react-hooks": reactHooks, "reactRefresh": reactRefresh },
        rules: { "react/prop-types": "off", }, // Ignore prop-types error, because we may not specify property types
    },
]);
