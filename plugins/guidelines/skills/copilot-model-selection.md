# Skill: Copilot Model Selection

**Description:** Select Azure Foundry model catalog deployments for JSdotNet Copilot work when Claude 5 catalog entries are blocked by provider-injected deprecated sampling parameters.

---

## Recommendation

Prefer **Azure Foundry model catalog/deployment entries** for Copilot sessions because Claude 5 catalog entries currently fail when the Anthropic provider injects deprecated `temperature`.

In the Copilot model UI, **Wire model** must exactly match the Azure Foundry deployment/model ID configured in Microsoft Foundry.

Configure these Azure Foundry models:

| Foundry display name | Wire model | Max prompt tokens | Max output tokens | Intended use |
|---|---|---:|---:|---|
| `gpt-5.4` | `gpt-5.4` | `922000` | `128000` | Default model |
| `gpt-5.5` | `gpt-5.5` | `922000` | `128000` | Premium fallback |
| `gpt-5.6-luna` | `gpt-5.6-luna` | `922000` | `128000` | Fast/cheaper routine work |
| `gpt-5.6-sol` | `gpt-5.6-sol` | `922000` | `128000` | Optional balanced alternative |

---

## Claude Fallbacks

Keep older Claude fallbacks only as Azure Foundry catalog entries, and configure them only when the Foundry catalog/provider accepts requests without the deprecated `temperature` parameter:

| Foundry display name | Wire model | Max prompt tokens | Max output tokens | Intended use |
|---|---|---:|---:|---|
| `claude-sonnet-4-6` | `claude-sonnet-4-6` | `1000000` | `128000` | Conditional premium Claude fallback |
| `claude-sonnet-4-5` | `claude-sonnet-4-5` | `200000` | `64000` | Conditional Claude fallback |
| `claude-haiku-4-5` | `claude-haiku-4-5` | `200000` | `64000` | Conditional fast Claude fallback |

Avoid Claude 5 models until the provider stops sending `temperature`:

- `claude-opus-5`
- `claude-sonnet-5`
- `claude-fable-5`

---

## Reasoning Model Parameter Safety

Do **not** configure sampling or legacy completion parameters for reasoning models. Leave these unset:

- `temperature`
- `top_p`
- frequency or presence penalties
- `logprobs`
- `logit_bias`
- `max_tokens`

Use model-specific prompt and output token limit fields instead of `max_tokens` where the host supports them.
