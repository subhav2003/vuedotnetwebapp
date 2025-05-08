<template>

  <div class="category-item bg-white shadow-lg rounded-lg overflow-hidden hover:shadow-xl transition-shadow duration-300">
    <img :src="category.image" :alt="category.name" class="w-full h-48 object-cover" />
    <div class="p-4">
      <h3 class="text-2xl font-bold font-papyrus text-brown-700">{{ category.name }}</h3>
      <p class="text-brown-600">{{ category.description }}</p>
    </div>
  </div>
</template>

<script>
import NavBar from "@/components/NavBar.vue";

export default {
  name: 'ProductCategory',
  components: {},
  props: {
    category: {
      type: Object,
      required: true,
    },
  },
};
</script>

<style scoped>
.category-item {
  transition: transform 0.3s ease;
}
.category-item:hover {
  transform: scale(1.05);
}
</style>
