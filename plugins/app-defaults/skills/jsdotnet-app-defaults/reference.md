# jsdotnet-app-defaults reference

The catalog the `jsdotnet-app-defaults` skill works over. `defaults/index.json` is the source;
`list_guides` returns the live list.

## Tools

| Tool | Returns |
|---|---|
| `list_guides` | Every document: id, title, description, category, path, tags |
| `list_guides_by_type(category)` | The documents of one category |
| `search_guides(query)` | Documents whose id, title or description match the text |
| `search_guides_by_tag(tag)` | Documents carrying the tag |
| `get_guide(id)` | One document's full markdown |
| `get_usage_logs(count)` | The latest tool calls this server process recorded |

Every document is also a resource at `defaults://<path without .md>`, for example
`defaults://arc42/adr/0003-solution-structure`.

## Categories

| Category | Holds |
|---|---|
| `adrs` | ADRs `0001`–`0013` and the numbering `README` |
| `designs` | arc42 chapters `02-constraints`, `04-solution-strategy`, `07-deployment-view`, `08-crosscutting-concepts` |
| `structures` | `05-building-block-view` and the building blocks: `module`, `domain-model`, `api`, `data-provider`, `apphost`, `service-defaults`, `test-harness`, `notifications`, `file-storage` |
| `recommendations` | `agent-skill-authoring`, `copilot-instruction-file-setup` |
| `config` | `config-mcp-json`, `config-github-app-yml` |
| `design` | The UX style guide: `readme`, `01-color-palette` … `09-interaction-patterns` |

## Where to start

| Question | Call |
|---|---|
| Solution, module or test layout | `get_guide("05-building-block-view")`, then the building block |
| Platform, build, packages | `get_guide("0001-platform-and-build")` |
| Aspire, configuration, secrets, IaC | `get_guide("0002-aspire-configuration-and-infrastructure")` |
| HTTP endpoints, validation, errors | `get_guide("0004-http-api")` |
| Handlers, `Result<T>`, time | `get_guide("0005-application-layer")` |
| EF Core, migrations, messaging | `get_guide("0006-persistence-and-messaging")` |
| Logging, tracing, resilience | `get_guide("0007-observability-and-resilience")` |
| Authentication, authorization, audit | `get_guide("0008-security")` |
| Frontend and tokens in code | `get_guide("0009-frontend")` |
| Test stack | `get_guide("0010-testing")` |
| An MCP server in an app | `get_guide("0011-setting-up-mcp")` |
| Agents inside an app | `get_guide("0012-agents-in-applications")` |
| Instruction files, `.mcp.json` | `get_guide("0013-agent-ready-repository")` |
| Caching, flags, idempotency, code style | `get_guide("08-crosscutting-concepts")` |

## Common tags

- Architecture: `modular-monolith`, `feature-slices`, `cqrs`, `ddd`, `domain-model`, `building-blocks`
- API and data: `minimal-api`, `openapi`, `problem-details`, `validation`, `ef-core`, `migrations`, `outbox`
- Platform: `aspire`, `apphost`, `service-defaults`, `configuration`, `secrets`, `deployment`
- Quality: `testing`, `playwright`, `opentelemetry`, `logging`, `resilience`, `security`, `zero-trust`
- Agents: `mcp`, `skills`, `agents-md`, `instructions`
- Design: `design-tokens`, `color`, `typography`, `spacing`, `motion`, `accessibility`, `customization`
