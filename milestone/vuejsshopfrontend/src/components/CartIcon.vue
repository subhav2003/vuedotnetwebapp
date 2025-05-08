<template>
  <div class="relative">
    <!-- Cart Icon -->
    <router-link to="/cart" class="inline-block">
      <font-awesome-icon icon="shopping-cart" class="text-brown-700 w-8 h-8 hover:text-brown-600" />
    </router-link>

    <!-- Badge: Conditional Cart Item Count -->
    <span
        v-if="totalItems > 0"
        class="absolute -top-1 -right-2 bg-red-500 text-white text-xs font-semibold rounded-full px-2 py-0.5 shadow-md"
    >
      {{ totalItems }}
    </span>
  </div>
</template>

<script setup>
import { useCart } from '@/store/useCart'; // Import the global cart state

// Get total items from the global cart state
const { totalItems } = useCart(); // Reactive totalItems computed property

</script>

<style scoped>
/* Scoped styles for additional customization if needed */
</style>
