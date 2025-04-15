# Automation Service: Diagrams

*Last Updated: 2025-04-14*

*Last updated: 2024-05-15*

This document contains diagrams and visual representations of the Automation Service architecture, trigger-action flows, rule evaluation processes, and integration patterns. These diagrams help visualize how the service works and how to use it effectively.

## Table of Contents

- [Service Architecture](#service-architecture)
- [Trigger-Rule-Action Flow](#trigger-rule-action-flow)
- [Rule Evaluation Process](#rule-evaluation-process)
- [Event Processing Pipeline](#event-processing-pipeline)
- [Integration with Functions Service](#integration-with-functions-service)
- [Webhook Processing](#webhook-processing)
- [Common Automation Patterns](#common-automation-patterns)

## Service Architecture

The following diagram illustrates the high-level architecture of the Automation Service:

```mermaid
flowchart TB
    subgraph External ["External Environment"]
        EventSources[Event Sources]
        ApiClients[API Clients]
        Webhooks[Webhook Sources]
        Blockchain[Neo N3 Blockchain]
    end

    subgraph Automation ["Automation Service"]
        ApiGateway[API Gateway]
        TriggerManager[Trigger Manager]
        RuleEngine[Rule Engine]
        ActionExecutor[Action Executor]
        EventIngestion[Event Ingestion]
        TriggerStorage[Trigger Storage]
        RuleStorage[Rule Storage]
        ExecutionHistory[Execution History]
        Scheduler[Scheduler]
    end

    subgraph Services ["Neo Service Layer"]
        functionservice[Functions Service]
        PriceFeedService[Price Feed Service]
        GasBankService[Gas Bank Service]
    end

    EventSources --> EventIngestion
    Webhooks --> ApiGateway
    ApiClients --> ApiGateway
    
    ApiGateway --> TriggerManager
    ApiGateway --> RuleEngine
    
    EventIngestion --> TriggerManager
    TriggerManager --> TriggerStorage
    TriggerManager --> RuleEngine
    
    RuleEngine --> RuleStorage
    RuleEngine --> ActionExecutor
    
    Scheduler --> TriggerManager
    Scheduler --> RuleEngine
    
    ActionExecutor --> ExecutionHistory
    ActionExecutor --> functionservice
    ActionExecutor --> Blockchain
    ActionExecutor --> ApiClients
    
    PriceFeedService --> EventIngestion
    functionservice --> EventIngestion
    GasBankService --> ActionExecutor
```

## Trigger-Rule-Action Flow

Diagram showing the relationship and flow between triggers, rules, and actions:

```mermaid
flowchart LR
    subgraph Triggers ["Triggers"]
        T1[Time-based Trigger]
        T2[Event-based Trigger]
        T3[Webhook Trigger]
        T4[Price Trigger]
        T5[Blockchain Trigger]
    end
    
    subgraph Rules ["Rules"]
        R1[Rule 1]
        R2[Rule 2]
        R3[Rule 3]
    end
    
    subgraph Conditions ["Conditions"]
        C1[Simple Condition]
        C2[Compound Condition]
        C3[Data Transformation]
    end
    
    subgraph Actions ["Actions"]
        A1[Function Call]
        A2[Webhook Call]
        A3[Blockchain Transaction]
        A4[Notification]
    end
    
    T1 --> R1
    T2 --> R1
    T3 --> R2
    T4 --> R2
    T5 --> R3
    
    R1 --> C1
    R2 --> C2
    R3 --> C3
    
    C1 --> A1
    C1 --> A2
    C2 --> A3
    C3 --> A4
    C3 --> A1
    
    classDef trigger fill:#f9f,stroke:#333,stroke-width:2px
    classDef rule fill:#bbf,stroke:#333,stroke-width:2px
    classDef condition fill:#fdb,stroke:#333,stroke-width:2px
    classDef action fill:#bfb,stroke:#333,stroke-width:2px
    
    class T1,T2,T3,T4,T5 trigger
    class R1,R2,R3 rule
    class C1,C2,C3 condition
    class A1,A2,A3,A4 action
```

## Rule Evaluation Process

This diagram shows how rules are evaluated:

```mermaid
sequenceDiagram
    participant Event as Event Source
    participant Trigger as Trigger System
    participant Rule as Rule Engine
    participant Condition as Condition Evaluator
    participant Action as Action Executor
    participant Function as Function Service
    
    Event->>Trigger: Generate Event
    Trigger->>Rule: Trigger Rule Evaluation
    Rule->>Condition: Evaluate Conditions
    
    alt Conditions Met
        Condition->>Rule: Conditions Satisfied
        Rule->>Action: Execute Actions
        
        alt Function Call
            Action->>Function: Execute Function
            Function->>Action: Function Result
        else Webhook Call
            Action->>Event: Call External Webhook
            Event->>Action: Webhook Response
        end
        
        Action->>Rule: Action Execution Result
        Rule->>Trigger: Rule Execution Complete
    else Conditions Not Met
        Condition->>Rule: Conditions Not Satisfied
        Rule->>Trigger: Rule Execution Skipped
    end
    
    Trigger->>Event: Event Processing Complete
```

## Event Processing Pipeline

Diagram showing how events are processed in the Automation Service:

```mermaid
flowchart TD
    subgraph Sources ["Event Sources"]
        TimeEvents[Time-based Events]
        APIEvents[API Events]
        WebhookEvents[Webhook Events]
        PriceEvents[Price Feed Events]
        BlockchainEvents[Blockchain Events]
    end
    
    subgraph Processing ["Event Processing"]
        Ingestion[Event Ingestion]
        Normalization[Event Normalization]
        Enrichment[Data Enrichment]
        Matching[Trigger Matching]
        Filtering[Event Filtering]
        Queue[Event Queue]
    end
    
    subgraph Evaluation ["Rule Evaluation"]
        RuleLoader[Rule Loading]
        ConditionEvaluation[Condition Evaluation]
        DataTransformation[Data Transformation]
        ActionSelection[Action Selection]
    end
    
    subgraph Execution ["Action Execution"]
        FunctionExecution[Function Execution]
        WebhookExecution[Webhook Execution]
        TransactionExecution[Transaction Execution]
        NotificationExecution[Notification Execution]
    end
    
    TimeEvents --> Ingestion
    APIEvents --> Ingestion
    WebhookEvents --> Ingestion
    PriceEvents --> Ingestion
    BlockchainEvents --> Ingestion
    
    Ingestion --> Normalization
    Normalization --> Enrichment
    Enrichment --> Matching
    Matching --> Filtering
    Filtering --> Queue
    
    Queue --> RuleLoader
    RuleLoader --> ConditionEvaluation
    ConditionEvaluation --> DataTransformation
    DataTransformation --> ActionSelection
    
    ActionSelection --> FunctionExecution
    ActionSelection --> WebhookExecution
    ActionSelection --> TransactionExecution
    ActionSelection --> NotificationExecution
    
    classDef source fill:#f9f,stroke:#333,stroke-width:2px
    classDef process fill:#fdb,stroke:#333,stroke-width:2px
    classDef evaluation fill:#bbf,stroke:#333,stroke-width:2px
    classDef execution fill:#bfb,stroke:#333,stroke-width:2px
    
    class TimeEvents,APIEvents,WebhookEvents,PriceEvents,BlockchainEvents source
    class Ingestion,Normalization,Enrichment,Matching,Filtering,Queue process
    class RuleLoader,ConditionEvaluation,DataTransformation,ActionSelection evaluation
    class FunctionExecution,WebhookExecution,TransactionExecution,NotificationExecution execution
```

## Integration with Functions Service

Visualization of how the Automation Service integrates with the Functions Service:

```mermaid
sequenceDiagram
    participant Event as Event Source
    participant Automation as Automation Service
    participant Functions as Functions Service
    participant TEE as Trusted Execution Environment
    participant Blockchain as Neo N3 Blockchain
    
    Event->>Automation: Trigger Event
    Automation->>Automation: Evaluate Rules
    
    alt Rule Conditions Met
        Automation->>Functions: Execute Function
        Functions->>TEE: Run in Secure Environment
        
        alt Function Success
            TEE->>Functions: Function Result
            Functions->>Automation: Execution Success + Result
            
            alt Result Requires Blockchain Action
                Automation->>Blockchain: Execute Transaction
                Blockchain->>Automation: Transaction Result
            end
        else Function Failure
            TEE->>Functions: Error Details
            Functions->>Automation: Execution Failure + Error
        end
        
        Automation->>Event: Action Result (optional)
    else Rule Conditions Not Met
        Automation->>Automation: Log Rule Skipped
    end
```

## Webhook Processing

Diagram showing how webhooks are processed:

```mermaid
sequenceDiagram
    participant External as External System
    participant API as API Gateway
    participant Validator as Webhook Validator
    participant Parser as Payload Parser
    participant RuleEngine as Rule Engine
    participant ActionExecutor as Action Executor
    
    External->>API: Webhook POST Request
    API->>Validator: Validate Webhook
    
    alt Valid Webhook
        Validator->>Parser: Parse Webhook Payload
        Parser->>RuleEngine: Trigger Rule Evaluation
        RuleEngine->>RuleEngine: Evaluate Conditions
        
        alt Conditions Met
            RuleEngine->>ActionExecutor: Execute Actions
            ActionExecutor->>API: Action Results
            API->>External: Success Response (200 OK)
        else Conditions Not Met
            RuleEngine->>API: No Actions Executed
            API->>External: Success Response (200 OK)
        end
    else Invalid Webhook
        Validator->>API: Validation Failed
        API->>External: Error Response (40x)
    end
```

## Common Automation Patterns

Visual representation of common automation patterns:

```mermaid
flowchart TD
    subgraph PatternA ["Scheduled Execution"]
        TimeTriggerA[Scheduled Trigger]
        FunctionA[Data Processing Function]
        ResultA[Data Storage]
    end
    
    subgraph PatternB ["Event-Driven Processing"]
        EventTriggerB[Event Trigger]
        RuleB[Filtering Rule]
        FunctionB[Event Handler Function]
        ResultB[Action Response]
    end
    
    subgraph PatternC ["Price Monitoring"]
        PriceTriggerC[Price Trigger]
        ConditionC[Threshold Condition]
        NotificationC[Notification Action]
        TradeC[Trading Action]
    end
    
    subgraph PatternD ["Smart Contract Automation"]
        BlockchainTriggerD[Blockchain Trigger]
        RuleD[Validation Rule]
        FunctionD[Transaction Function]
        ResultD[Contract Interaction]
    end
    
    TimeTriggerA --> FunctionA
    FunctionA --> ResultA
    
    EventTriggerB --> RuleB
    RuleB --> FunctionB
    FunctionB --> ResultB
    
    PriceTriggerC --> ConditionC
    ConditionC --> NotificationC
    ConditionC --> TradeC
    
    BlockchainTriggerD --> RuleD
    RuleD --> FunctionD
    FunctionD --> ResultD
    
    classDef trigger fill:#f9f,stroke:#333,stroke-width:2px
    classDef process fill:#bbf,stroke:#333,stroke-width:2px
    classDef result fill:#bfb,stroke:#333,stroke-width:2px
    
    class TimeTriggerA,EventTriggerB,PriceTriggerC,BlockchainTriggerD trigger
    class FunctionA,RuleB,FunctionB,ConditionC,RuleD,FunctionD process
    class ResultA,ResultB,NotificationC,TradeC,ResultD result
```

## Using These Diagrams

These diagrams can be rendered in GitHub and other platforms that support Mermaid markdown. You can also copy the Mermaid code and use it in tools like:

- [Mermaid Live Editor](https://mermaid.live/)
- [GitHub Repositories](https://docs.github.com/en/get-started/writing-on-github/working-with-advanced-formatting/creating-diagrams)
- Documentation tools that support Mermaid

To embed these diagrams in other documentation, simply copy the Mermaid code blocks.
